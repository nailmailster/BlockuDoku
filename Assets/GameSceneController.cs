using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameSceneController : MonoBehaviour
{
    public Camera gameCamera;

    //  этот префаб пока не используется (вроде бы)
    public GameObject cellPrefab;

    //  это префаб в чистом виде - он используются только для того, чтобы по его шаблону создать игровую доску
    public GameObject boardPrefab;

    //  это префабы в чистом виде - они используются только для того, чтобы по их шаблону создавать новые фигуры
    //  в дальнейгем таких префабов станет столько, сколько вариантов фигур будут использоваться в игре
    //  вопрос о том, что часть фигур представляют собой вариации других, оставим на потом
    public GameObject figure1Prefab, figure2Prefab, figure3Prefab, figure4Prefab, figure5Prefab,
                    figure6Prefab, figure7Prefab, figure8Prefab, figure9Prefab, figure10Prefab,
                    figure11Prefab, figure12Prefab, figure13Prefab, figure14Prefab, figure15Prefab,
                    figure16Prefab, figure17Prefab, figure18Prefab, figure19Prefab, figure20Prefab,
                    figure21Prefab, figure22Prefab, figure23Prefab, figure24Prefab, figure25Prefab,
                    figure26Prefab, figure27Prefab, figure28Prefab, figure29Prefab, figure30Prefab,
                    figure31Prefab, figure32Prefab, figure33Prefab, figure34Prefab, figure35Prefab,
                    figure36Prefab, figure37Prefab, figure38Prefab, figure39Prefab;

    //  ссылка на объект Text в Canvas
    public Text debugText, debugText2, scoreText, bestScoreText;

    //  объект игровой доски
    private GameObject board;
    //  матрица объектов игровой доски
    private GameObject[,] boardMatrice = new GameObject[9, 9];

    //  объекты фигур, доступных игроку
    private GameObject figure1, figure2, figure3;
    private GameObject activeFigure;

    //  мировое положение фигуры во время ее перемещения
    private Vector2 pos;
    private Vector2Int closest;

    private List<Vector2Int> figure1Matrix = new List<Vector2Int>(), figure2Matrix = new List<Vector2Int>(), figure3Matrix = new List<Vector2Int>();
    private List<int> figure1Colors = new List<int>(), figure2Colors = new List<int>(), figure3Colors = new List<int>();
    private List<Vector2Int> activeFigureMatrix;
    private List<int> activeFigureColors = new List<int>();

    bool figure1ToDestroy = false, figure2ToDestroy = false, figure3ToDestroy = false;

    int score = 0, bestScore;

    // Color[] colors = new Color[6] { Color.blue, Color.red, Color.green, Color.yellow, Color.magenta, Color.grey };
    //                                     0           1           2            3             4             5
    Color[] colors = new Color[6] { Color.blue, Color.red, Color.yellow, Color.white, Color.green, Color.magenta };

    bool isGameOver;

    Vector2 mouseDownPosition, mouseUpPosition;
    float deltaX, deltaY;
    Vector2Int delta;

    int FileIDMouseDown, SoundIDMouseDown;
    int FileIDMouseUp, SoundIDMouseUp;
    int FileIDWinners, SoundIDWinners;

    void Awake()
    {
        isGameOver = false;

        Color blue = new Color32(50, 50, 220, 255);
        colors[0] = blue;
        Color red = new Color32(220, 50, 50, 255);
        colors[1] = red;
        Color yellow = new Color32(220, 220, 50, 255);
        colors[2] = yellow;
        Color white = new Color32(220, 220, 220, 255);
        colors[3] = white;

        bestScore = PlayerPrefs.GetInt("bestScore", 0);
    }

    void Start()
    {
        AndroidNativeAudio.makePool(1);
        FileIDMouseDown = AndroidNativeAudio.load("Android Native Audio/MouseDown.wav");
        FileIDMouseUp = AndroidNativeAudio.load("Android Native Audio/MouseUp.wav");
        // FileIDWinners = AndroidNativeAudio.load("Android Native Audio/Winners.wav");
        FileIDWinners = AndroidNativeAudio.load("Android Native Audio/Phew.wav");

        //  рисуем доску
        BuildBoard();

        GenerateNewFigures();
        bestScoreText.text = bestScore.ToString();
    }

	void OnApplicationQuit()
	{
		AndroidNativeAudio.unload(FileIDMouseDown);
		AndroidNativeAudio.unload(FileIDMouseUp);
		AndroidNativeAudio.unload(FileIDWinners);
		AndroidNativeAudio.releasePool();
	}

    void AttachMouseEventsOnCells()
    {
        for (int x = 0; x < 9; x++)
        {
            for (int y = 0; y < 9; y++)
            {
                if (boardMatrice[x, y].name != null && boardMatrice[x, y].name.Substring(0, 4) == "Blue")
                {
                    boardMatrice[x, y].gameObject.GetComponent<CellScript>().OnMDown -= OnCellMDown;
                    boardMatrice[x, y].gameObject.GetComponent<CellScript>().OnMDrag -= OnCellMDrag;
                    boardMatrice[x, y].gameObject.GetComponent<CellScript>().OnMUp -= OnCellMUp;

                    boardMatrice[x, y].gameObject.GetComponent<CellScript>().OnMDown += OnCellMDown;
                    boardMatrice[x, y].gameObject.GetComponent<CellScript>().OnMDrag += OnCellMDrag;
                    boardMatrice[x, y].gameObject.GetComponent<CellScript>().OnMUp += OnCellMUp;
                }
            }
        }
    }

    void OnCellMDown(GameObject cell, Vector2 mousePosition)
    {
        mouseDownPosition = mousePosition;
        // print(cell.name + "   coordinates = " + cell.GetComponent<CellScript>().coordinates + "     mousePosition " + mousePosition);
    }

    void OnCellMDrag(GameObject cell, Vector2 mousePosition)
    {
        mouseUpPosition = mousePosition;
        // print("OnCellMDrag  mousePosition " + mousePosition);
    }

    //  отпустили ячейку после перетаскивания - на входе: объект матрицы доски и мировые координаты мыши
    void OnCellMUp(GameObject cell, Vector2 mousePosition)
    {
        //  определяем смещение мыши с момента когда ячейка была выбрана для перемещения
        deltaX = mouseUpPosition.x - mouseDownPosition.x;
        deltaY = mouseUpPosition.y - mouseDownPosition.y;

        //  если ячейку двигали по горизонтали
        if (Mathf.Abs(deltaX) >= Mathf.Abs(deltaY))
        {
            //  нормализуем горизонтальное смещение до единицы и обнуляем вертикальное смещение
            deltaY = 0;
            if (deltaX > 0)
                deltaX = 1;
            else
                deltaX = -1;
        }
        //  если ячейку двигали по вертикали
        else if (Mathf.Abs(deltaY) > Mathf.Abs(deltaX))
        {
            //  нормализуем вертикальное смещение до единицы и обнуляем горизонтальное смещение
            deltaX = 0;
            if (deltaY > 0)
                deltaY = 1;
            else
                deltaY = -1;
        }
        //  запоминаем нормализованное направление ячейки
        delta = new Vector2Int((int)deltaX, (int)deltaY);

        //  считываем координаты ячейки в матрице доски
        Vector2Int cellCoordinates = cell.GetComponent<CellScript>().coordinates;
        //  если направление перемещения ячейки не выводит ее за пределы игровой доски
        if ((cellCoordinates.x + delta.x) >= 0 && (cellCoordinates.x + delta.x) <= 8
            && (cellCoordinates.y + delta.y) >= 0 && (cellCoordinates.y + delta.y) <= 8)
        {
            //  двинем ее в заданном направлении
            MoveIt(cell, delta, cellCoordinates);
        }
    }

    //  двигаем ячейку cell с координатами cellCoordinates в направлении вектора delta
    void MoveIt(GameObject cell, Vector2Int delta, Vector2Int cellCoordinates)
    {
        //  подвинем ячейку с помощтю корутины
        StartCoroutine(MoveItRoutine(cell, delta));
    }

    //  корутина перемещения ячейки cell с координатами cellCoordinates в направлении delta
    private IEnumerator MoveItRoutine(GameObject cell, Vector2Int delta)
    {
        //  результат свопинга запомним в переменной
        int exchangeResult = board.GetComponent<BoardScript>().Exchange(boardMatrice, cell, delta);

        //  дадаим время для анимации
        yield return new WaitForSeconds(0.3f);

        //  ячейки поменялись местами
        if (exchangeResult == 1)
        {
            //  проверимЮ дало ли перемещение выигрышный результат
            int result = board.GetComponent<BoardScript>().SeekWinnersAndDestroy(boardMatrice);

            //  если перемещение ячеек не дало выигрышного результата
            if (result == 0)
            {
                //  запомним новые координаты ячейки
                Vector2Int newCellCoordinates = new Vector2Int(cell.GetComponent<CellScript>().coordinates.x, cell.GetComponent<CellScript>().coordinates.y);
                //  считаем перемещенную ячейку из матрицы доски
                GameObject newCell = boardMatrice[newCellCoordinates.x, newCellCoordinates.y];
                //  запомним результат обратного перемещения
                exchangeResult = board.GetComponent<BoardScript>().Exchange(boardMatrice, newCell, delta * -1);
                //  обратное перемещение прошло неудачно - уведомим разработчика об ошибочном поведении
                if (exchangeResult != 1)
                    Debug.Log("exchangeResult = " + exchangeResult);
            }
            //  иначе если перемещение ячеек дало выигрышный результат
            else
            {
                //  увеличиваем текущий счет
                score += result;
                //  обновляем текущий счет в интерфейсе
                scoreText.text = score.ToString();
                //  проверим, имеется ли у игрока возможность сделать ход
                CheckFiguresForMovesAvailability();
            }
        }
        //  попытка сдвинуть ячейку на пустое поле
        else if (exchangeResult == -1)
        {
        }
        yield return null;
    }

    //  по нажатию на фигуру
    void OnMDown(GameObject figureGameObject)
    {
        //  воспроизведем нажатие мыши
        SoundIDMouseDown = AndroidNativeAudio.play(FileIDMouseDown);

        if (figureGameObject.gameObject.name.Equals("Figure1"))
        {
            activeFigureColors = figure1Colors;
            activeFigureMatrix = figure1Matrix;
        }
        else if (figureGameObject.gameObject.name.Equals("Figure2"))
        {
            activeFigureColors = figure2Colors;
            activeFigureMatrix = figure2Matrix;
        }
        else if (figureGameObject.gameObject.name.Equals("Figure3"))
        {
            activeFigureColors = figure3Colors;
            activeFigureMatrix = figure3Matrix;
        }
        //  увеличиваем ее и приподнимаем вверх
        figureGameObject.GetComponent<FigureScript>().ScaleFigureAndChildren("zoom-in", activeFigureColors);
        activeFigure = figureGameObject;
    }

    //  при перетаскивании фигуры
    //  worldPos == 0 когда фигура в центре доски
    void OnMDrag(GameObject figureGameObject, Vector2 mousePos, Vector2 worldPos, Vector3 transPos)
    {
        // pos = worldPos;  //  worldPos на три клетки выше
        pos = transPos;
        //  определяем ближайшую к центру фигуры свободную клетку на доске
        closest = board.GetComponent<BoardScript>().GetClosest(activeFigure, pos);

        //  удаляем все тени
        board.GetComponent<BoardScript>().ClearShadows(boardMatrice, closest, true);


        //  возможно, в предыдущем положении фигуры создавалась выигрышная ситуация, поэтому установим оригинальную яркость всем непустым ячейкам доски
        board.GetComponent<BoardScript>().ClearHighlights(boardMatrice);

        //  если на игровой доске поблизости от активной фигуры имеется свободное место для нее
        if (CanMove())
        {
            //  уничтожаем содержимое ячеек доски для последующего заполнения префабами тени фигуры
            foreach (Vector2Int matrixPos in activeFigureMatrix)
                if (boardMatrice[closest.x + matrixPos.x, closest.y + matrixPos.y] != null)
                    Destroy(boardMatrice[closest.x + matrixPos.x, closest.y + matrixPos.y].gameObject);
            //  рисуем тень фигуры в ближайшем подходящем месте от центра
            board.GetComponent<BoardScript>().DrawShadow(boardMatrice, closest, activeFigureMatrix);

            //  Поместим фигуру на виртуальную доску (на самом деле заполним только матрицу цветов)
            board.GetComponent<BoardScript>().DrawFakeFigureInsteadOfShadow(closest, activeFigureMatrix, activeFigureColors);
            //  подсвечиваем клетки доски, которые выиграют если фигуру разместить в этом положении
            board.GetComponent<BoardScript>().SeekWinnersAndHighlight(boardMatrice);
        }
    }

    //  при отпускании фигуры
    void OnMUp(GameObject figureGameObject)
    {
        //  воспроизведем отпускание мыши
        SoundIDMouseUp = AndroidNativeAudio.play(FileIDMouseUp);

        if (activeFigure.gameObject.name.Equals("Figure1"))
            activeFigureColors = figure1Colors;
        else if (activeFigure.gameObject.name.Equals("Figure2"))
            activeFigureColors = figure2Colors;
        else if (activeFigure.gameObject.name.Equals("Figure3"))
            activeFigureColors = figure3Colors;
        //  если на игровой доске поблизости от активной фигуры имеется свободное место для нее
        if (CanMove())
        {
            //  попробуем такой вариант: заменим тени на элементы фигуры
            board.GetComponent<BoardScript>().DrawFigureInsteadOfShadow(boardMatrice, closest, activeFigureMatrix, activeFigureColors);
            AttachMouseEventsOnCells();

            if (activeFigure.gameObject.name.Equals("Figure1"))
            {
                Destroy(figure1.gameObject);
                figure1ToDestroy = true;
            }
            else if (activeFigure.gameObject.name.Equals("Figure2"))
            {
                Destroy(figure2.gameObject);
                figure2ToDestroy = true;
            }
            else if (activeFigure.gameObject.name.Equals("Figure3"))
            {
                Destroy(figure3.gameObject);
                figure3ToDestroy = true;
            }
            activeFigure = null;

            //  запомним текущий результат
            int previousScore = score;
            score += board.GetComponent<BoardScript>().SeekWinnersAndDestroy(boardMatrice);
            scoreText.text = score.ToString();
            //  если текущий результат увеличился
            if (score > previousScore)
                //  воспроизведем new high score
                SoundIDWinners = AndroidNativeAudio.play(FileIDWinners);

            //  если отпущена последняя фигура, а две другие были уже использованы ранее
            if ((figure1 == null && figure2 == null) || (figure1 == null && figure3 == null) || (figure2 == null && figure3 == null))
            {
                //  то проверять ходы не для чего
            }
            //  иначе если еще остались фигуры
            else
                //  проверим, есть ли у них ходы
                CheckFiguresForMovesAvailability();
        }
        //  если свободного места для фигуры нет
        else
        {
            // board.GetComponent<BoardScript>().ClearOneShadow(boardMatrice, closest);
            //  очищаем доску от всех теней
            board.GetComponent<BoardScript>().ClearShadows(boardMatrice, closest, true);
            //  уменьшаем фигуру
            figureGameObject.GetComponent<FigureScript>().ScaleFigureAndChildren("zoom-out", activeFigureColors);
            //  возвращаем фигуру на исходную позицию
            iTween.MoveTo(figureGameObject, iTween.Hash(
                "position", figureGameObject.GetComponent<FigureScript>().originalFigurePosition,
                "time", 0.3f));

            //  устанавливаем каждой клетке фигуры исходный цвет
            for (int i = 0; i < figureGameObject.transform.childCount; i++)
                // figureGameObject.transform.GetChild(i).GetComponent<SpriteRenderer>().color = new Color32(50, 50, 255, 255);
                figureGameObject.transform.GetChild(i).GetComponent<SpriteRenderer>().color = colors[activeFigureColors[i]];
        }
    }

    //  определяет, есть ли на игровой доске поблизости от фигуры свободное место для нее
    bool CanMove()
    {
        if (closest.x == 100 || closest.y == 100)
        {
            // Debug.Log("CanNotMove   closest = " + closest);
            return false;
        }
        if (boardMatrice[closest.x, closest.y] != null && boardMatrice[closest.x, closest.y].gameObject.name.Substring(0, 4).Equals("Blue"))
        {
            // Debug.Log("CanNotMove   boardMatrice[closest.x, closest.y] = " + boardMatrice[closest.x, closest.y] + "   boardMatrice[closest.x, closest.y].gameObject.name.Substring(0, 4) = " + boardMatrice[closest.x, closest.y].gameObject.name.Substring(0, 4));
            return false;
        }
        foreach(Vector2Int matrixPos in activeFigureMatrix)
        {
            if (closest.x + matrixPos.x < 0 || closest.x + matrixPos.x > 8 || closest.y + matrixPos.y < 0 || closest.y + matrixPos.y > 8)
            {
                // Debug.Log("CanNotMove   out of GameBoard    closest.x + matrixPos.x = " + (closest.x + matrixPos.x) + "   closest.y + matrixPos.y = " + (closest.y + matrixPos.y));
                return false;
            }
            if (boardMatrice[closest.x + matrixPos.x, closest.y + matrixPos.y] != null && boardMatrice[closest.x + matrixPos.x, closest.y + matrixPos.y].gameObject.name.Substring(0, 4).Equals("Blue"))
            {
                // Debug.Log("CanNotMove   other reason    boardMatrice[closest.x + matrixPos.x, closest.y + matrixPos.y] = " + boardMatrice[closest.x + matrixPos.x, closest.y + matrixPos.y] + " name = " + boardMatrice[closest.x + matrixPos.x, closest.y + matrixPos.y].name);
                // Debug.Log("CanNotMove   closest = " + closest);
                // Debug.Log("CanNotMove   matrixPos = " + matrixPos);
                return false;
            }
        }
        return true;
    }

    private bool CouldMove(GameObject figure, List<Vector2Int> matrix)
    {
        activeFigure = figure;
        activeFigureMatrix = matrix;
        for (int x = 0; x < 9; x++)
        {
            for (int y = 0; y < 9; y++)
            {
                closest.x = x;
                closest.y = y;
                if (CanMove())
                    return true;
            }
        }

        return false;
    }

    //  проверяем все фигуры на возможность сделать ход
    private void CheckFiguresForMovesAvailability()
    {
        //  если ни для одной фигуры нет возможности сделать ход, игра будет считаться оконченной
        isGameOver = true;

        //  если первая фигура еще не использована
        if (!figure1ToDestroy && figure1 != null)
        {
            //  проверяем ходы
            figure1.GetComponent<FigureScript>().couldMove = CouldMove(figure1, figure1Matrix);
            //  если ходов нет
            if (!figure1.GetComponent<FigureScript>().couldMove)
            {
                //  устанавливаем всем клеткам фигуры цвет недоступности
                for (int i = 0; i < figure1.transform.childCount; i++)
                    figure1.transform.GetChild(i).GetComponent<SpriteRenderer>().color = new Color32(130, 130, 255, 255);
            }
            else
            {
                //  устанавливаем всем клеткам фигуры исходный цвет
                for (int i = 0; i < figure1.transform.childCount; i++)
                {
                    // figure1.transform.GetChild(i).GetComponent<SpriteRenderer>().color = new Color32(50, 50, 255, 255);
                    figure1.gameObject.transform.GetChild(i).GetComponent<SpriteRenderer>().color = colors[figure1Colors[i]];
                }
                //  игра продолжается
                isGameOver = false;
            }
        }

        //  если вторая фигура еще не использована
        if (!figure2ToDestroy && figure2 != null)
        {
            //  проверяем ходы
            figure2.GetComponent<FigureScript>().couldMove = CouldMove(figure2, figure2Matrix);
            //  если ходов нет
            if (!figure2.GetComponent<FigureScript>().couldMove)
            {
                //  устанавливаем всем клеткам фигуры цвет недоступности
                for (int i = 0; i < figure2.transform.childCount; i++)
                    figure2.transform.GetChild(i).GetComponent<SpriteRenderer>().color = new Color32(130, 130, 255, 255);
            }
            else
            {
                //  устанавливаем всем клеткам фигуры исходный цвет
                for (int i = 0; i < figure2.transform.childCount; i++)
                {
                    // figure2.transform.GetChild(i).GetComponent<SpriteRenderer>().color = new Color32(50, 50, 255, 255);
                    figure2.gameObject.transform.GetChild(i).GetComponent<SpriteRenderer>().color = colors[figure2Colors[i]];
                }
                //  игра продолжается
                isGameOver = false;
            }
        }

        //  если третья фигура еще не использована
        if (!figure3ToDestroy && figure3 != null)
        {
            //  проверяем ходы
            figure3.GetComponent<FigureScript>().couldMove = CouldMove(figure3, figure3Matrix);
            //  если ходов нет
            if (!figure3.GetComponent<FigureScript>().couldMove)
            {
                //  устанавливаем всем клеткам фигуры цвет недоступности
                for (int i = 0; i < figure3.transform.childCount; i++)
                    figure3.transform.GetChild(i).GetComponent<SpriteRenderer>().color = new Color32(130, 130, 255, 255);
            }
            else
            {
                //  устанавливаем всем клеткам фигуры исходный цвет
                for (int i = 0; i < figure3.transform.childCount; i++)
                {
                    // figure3.transform.GetChild(i).GetComponent<SpriteRenderer>().color = new Color32(50, 50, 255, 255);
                    figure3.gameObject.transform.GetChild(i).GetComponent<SpriteRenderer>().color = colors[figure3Colors[i]];
                }
                //  игра продолжается
                isGameOver = false;
            }
        }

        //  если проверка пришлась на тот момент, когда все фигуры были использованы, но еще не успели удалиться
        if (figure1ToDestroy && figure2ToDestroy && figure3ToDestroy)
            //  игра продолжается
            isGameOver = false;

        //  если игра окончена
        if (isGameOver)
        {
            //  проверим match3
            if (CouldSwap())
                //  если можно свапить, игра продолжается
                isGameOver = false;
        }

        //  если игра точно окончена
        if (isGameOver)
        {
            //  выводим UI
            debugText.fontSize = 0;
            debugText.text = "GAME OVER".ToLower();
            debugText2.text = "GAME OVER".ToLower();
            isGameOver = false;
            StartCoroutine("ScaleFont");
            if (score > bestScore)
            {
                bestScore = score;
                PlayerPrefs.SetInt("bestScore", bestScore);
            }
            isGameOver = true;
        }
        else
        {
            debugText.text = "";
            debugText2.text = "";
        }
    }

    //  проверка, есть ли ходы свапом
    private bool CouldSwap()
    {
        // Debug.Log("Checking");
        //  переберем все ячейки доски
        for (int x = 0; x < 9; x++)
        {
            for (int y = 0; y < 9; y++)
            {
                string name = boardMatrice[x, y].name.Substring(0, 4);
                //  нас интересуют только заполненные поля
                if (name.Equals("Blue"))
                {
                    //  получим список соседей нашей ячейки
                    List<Vector2Int> neighbours = GiveNeighbours(x, y);
                    //  если есть хотя бы один сосед
                    if (neighbours.Count > 0)
                    {
                        //  проверим, есть ли у нашей ячейки победители свапа с этим соседом
                        if (board.GetComponent<BoardScript>().CheckSwapWinners(x, y, neighbours))
                        {
                            //  если хотя бы один свап дает победителей, проверку прекращаем и возвращаем true
                            // Debug.Log("swap is available on " + x + ", " + y);
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    private List<Vector2Int> GiveNeighbours(int X, int Y)
    {
        List<Vector2Int> neighbours = new List<Vector2Int>();

        // if (X > 0 && boardMatrice[X - 1, Y].name.Substring(0, 4).Equals("Blue"))
        //     neighbours.Add(new Vector2Int(X - 1, Y));

        // if (X < 8 && boardMatrice[X + 1, Y].name.Substring(0, 4).Equals("Blue"))
        //     neighbours.Add(new Vector2Int(X + 1, Y));

        // if (Y > 0 && boardMatrice[X, Y - 1].name.Substring(0, 4).Equals("Blue"))
        //     neighbours.Add(new Vector2Int(X, Y - 1));

        // if (Y < 8 && boardMatrice[X, Y + 1].name.Substring(0, 4).Equals("Blue"))
        //     neighbours.Add(new Vector2Int(X, Y + 1));

        if (X > 0)
            neighbours.Add(new Vector2Int(X - 1, Y));

        if (X < 8)
            neighbours.Add(new Vector2Int(X + 1, Y));

        if (Y > 0)
            neighbours.Add(new Vector2Int(X, Y - 1));

        if (Y < 8)
            neighbours.Add(new Vector2Int(X, Y + 1));

        return neighbours;
    }

    //  корутина увеличения размера фонта UI
    IEnumerator ScaleFont()
    {
        // for (int size = 0; size <= 132; size += 2)
        for (int size = 0; size <= 100; size ++)
        {
            debugText.fontSize = size;
            debugText2.fontSize = size;
            yield return new WaitForSeconds(.01f);
        }
    }

    void Update()
    {
        if (figure1.gameObject == null && figure2.gameObject == null && figure3.gameObject == null)
        {
            GenerateNewFigures();
        }

        if (figure1ToDestroy)
        {
            figure1ToDestroy = false;
        }
        if (figure2ToDestroy)
        {
            figure2ToDestroy = false;
        }
        if (figure3ToDestroy)
        {
            figure3ToDestroy = false;
        }

        if (Input.GetKey("escape"))
            Application.Quit();
        
        if (isGameOver)
        {
            var fingerCount = 0;
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled)
                {
                    fingerCount++;
                }
            }
            if (fingerCount > 0)
            {
                isGameOver = false;
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }

    private void GenerateNewFigures()
    {
        int randomFigurePrefabIndex = Random.Range(1, 40);
        CreateFigureFromPrefab(1, randomFigurePrefabIndex);
        figure1ToDestroy = false;

        randomFigurePrefabIndex = Random.Range(1, 40);
        CreateFigureFromPrefab(2, randomFigurePrefabIndex);
        figure2ToDestroy = false;

        randomFigurePrefabIndex = Random.Range(1, 40);
        CreateFigureFromPrefab(3, randomFigurePrefabIndex);
        figure3ToDestroy = false;

        CheckFiguresForMovesAvailability();
    }

    //  остались нереализованными вариации префабов фигур 6, 7, 8, 10 и 11
    private void CreateFigureFromPrefab(int figureIndex, int prefabIndex)
    {
        List<Vector2Int> figureMatrix = new List<Vector2Int>();
        GameObject figurePrefab;

        switch (prefabIndex)
        {
            case 1:
                figureMatrix.Add(new Vector2Int(0, 0));
                figureMatrix.Add(new Vector2Int(0, -1));
                figureMatrix.Add(new Vector2Int(0, 1));
                figureMatrix.Add(new Vector2Int(1, 1));

                figurePrefab = figure1Prefab;

                break;
            case 2:
                figureMatrix.Add(new Vector2Int(0, 0));

                figurePrefab = figure2Prefab;

                break;
            case 3:
                figureMatrix.Add(new Vector2Int(0, 0));
                figureMatrix.Add(new Vector2Int(0, -1));
                figureMatrix.Add(new Vector2Int(0, 1));
                figureMatrix.Add(new Vector2Int(-1, 1));
                figureMatrix.Add(new Vector2Int(1, 1));

                figurePrefab = figure3Prefab;

                break;
            case 4:
                figureMatrix.Add(new Vector2Int(0, 0));
                figureMatrix.Add(new Vector2Int(0, -1));
                figureMatrix.Add(new Vector2Int(0, 1));
                figureMatrix.Add(new Vector2Int(-1, 0));
                figureMatrix.Add(new Vector2Int(1, 0));

                figurePrefab = figure4Prefab;

                break;
            case 5:
                figureMatrix.Add(new Vector2Int(0, 0));
                figureMatrix.Add(new Vector2Int(0, -1));
                figureMatrix.Add(new Vector2Int(-1, -1));
                figureMatrix.Add(new Vector2Int(1, 0));

                figurePrefab = figure5Prefab;

                break;
            case 6:
                figureMatrix.Add(new Vector2Int(0, 0));
                figureMatrix.Add(new Vector2Int(0, -1));
                figureMatrix.Add(new Vector2Int(1, 0));

                figurePrefab = figure6Prefab;

                break;
            case 7:
                figureMatrix.Add(new Vector2Int(0, 0));
                figureMatrix.Add(new Vector2Int(0, -1));
                figureMatrix.Add(new Vector2Int(0, 1));

                figurePrefab = figure7Prefab;

                break;
            case 8:
                figureMatrix.Add(new Vector2Int(0, 0));
                figureMatrix.Add(new Vector2Int(0, 1));
                figureMatrix.Add(new Vector2Int(-1, 0));
                figureMatrix.Add(new Vector2Int(1, 0));

                figurePrefab = figure8Prefab;

                break;
            case 9:
                figureMatrix.Add(new Vector2Int(0, 0));
                figureMatrix.Add(new Vector2Int(0, 1));
                figureMatrix.Add(new Vector2Int(1, 0));
                figureMatrix.Add(new Vector2Int(1, 1));

                figurePrefab = figure9Prefab;

                break;
            case 10:
                figureMatrix.Add(new Vector2Int(0, 0));
                figureMatrix.Add(new Vector2Int(0, -1));
                figureMatrix.Add(new Vector2Int(0, 1));
                figureMatrix.Add(new Vector2Int(1, 1));
                figureMatrix.Add(new Vector2Int(2, 1));

                figurePrefab = figure10Prefab;

                break;
            case 11:
                figureMatrix.Add(new Vector2Int(0, 3));
                figureMatrix.Add(new Vector2Int(0, 1));
                figureMatrix.Add(new Vector2Int(0, 2));
                figureMatrix.Add(new Vector2Int(0, 0));
                figureMatrix.Add(new Vector2Int(0, -1));

                figurePrefab = figure11Prefab;

                break;
            case 12:
                figureMatrix.Add(new Vector2Int(0, 0));
                figureMatrix.Add(new Vector2Int(1, 1));

                figurePrefab = figure12Prefab;

                break;
            case 13:
                figureMatrix.Add(new Vector2Int(0, 0));
                figureMatrix.Add(new Vector2Int(1, 1));
                figureMatrix.Add(new Vector2Int(-1, -1));

                figurePrefab = figure13Prefab;

                break;
            case 14:
                figureMatrix.Add(new Vector2Int(-1, 1));
                figureMatrix.Add(new Vector2Int(0, 0));

                figurePrefab = figure14Prefab;

                break;
            case 15:
                figureMatrix.Add(new Vector2Int(0, 0));
                figureMatrix.Add(new Vector2Int(-1, 1));
                figureMatrix.Add(new Vector2Int(1, -1));

                figurePrefab = figure15Prefab;

                break;
            case 16:
                figureMatrix.Add(new Vector2Int(0, 0));
                figureMatrix.Add(new Vector2Int(0, -1));
                figureMatrix.Add(new Vector2Int(0, 1));
                figureMatrix.Add(new Vector2Int(-1, -1));
                figureMatrix.Add(new Vector2Int(1, -1));

                figurePrefab = figure16Prefab;

                break;
            case 17:
                figureMatrix.Add(new Vector2Int(0, 0));
                figureMatrix.Add(new Vector2Int(1, -1));
                figureMatrix.Add(new Vector2Int(1, 1));
                figureMatrix.Add(new Vector2Int(-1, 0));
                figureMatrix.Add(new Vector2Int(1, 0));

                figurePrefab = figure17Prefab;

                break;
            case 18:
                figureMatrix.Add(new Vector2Int(0, 0));
                figureMatrix.Add(new Vector2Int(-1, -1));
                figureMatrix.Add(new Vector2Int(-1, 1));
                figureMatrix.Add(new Vector2Int(-1, 0));
                figureMatrix.Add(new Vector2Int(1, 0));

                figurePrefab = figure18Prefab;

                break;
            case 19:
                figureMatrix.Add(new Vector2Int(0, 0));
                figureMatrix.Add(new Vector2Int(0, -1));
                figureMatrix.Add(new Vector2Int(0, 1));
                figureMatrix.Add(new Vector2Int(1, -1));

                figurePrefab = figure19Prefab;

                break;
            case 20:
                figureMatrix.Add(new Vector2Int(0, 0));
                figureMatrix.Add(new Vector2Int(0, -1));
                figureMatrix.Add(new Vector2Int(0, 1));
                figureMatrix.Add(new Vector2Int(-1, -1));

                figurePrefab = figure20Prefab;

                break;
            case 21:
                figureMatrix.Add(new Vector2Int(0, 0));
                figureMatrix.Add(new Vector2Int(0, -1));
                figureMatrix.Add(new Vector2Int(0, 1));
                figureMatrix.Add(new Vector2Int(-1, 1));

                figurePrefab = figure21Prefab;

                break;
            case 22:
                figureMatrix.Add(new Vector2Int(0, 0));
                figureMatrix.Add(new Vector2Int(-1, -1));
                figureMatrix.Add(new Vector2Int(-1, 0));
                figureMatrix.Add(new Vector2Int(1, 0));

                figurePrefab = figure22Prefab;

                break;
            case 23:
                figureMatrix.Add(new Vector2Int(0, 0));
                figureMatrix.Add(new Vector2Int(-1, 1));
                figureMatrix.Add(new Vector2Int(-1, 0));
                figureMatrix.Add(new Vector2Int(1, 0));

                figurePrefab = figure23Prefab;

                break;
            case 24:
                figureMatrix.Add(new Vector2Int(0, 0));
                figureMatrix.Add(new Vector2Int(1, -1));
                figureMatrix.Add(new Vector2Int(-1, 0));
                figureMatrix.Add(new Vector2Int(1, 0));

                figurePrefab = figure24Prefab;

                break;
            case 25:
                figureMatrix.Add(new Vector2Int(0, 0));
                figureMatrix.Add(new Vector2Int(1, 1));
                figureMatrix.Add(new Vector2Int(-1, 0));
                figureMatrix.Add(new Vector2Int(1, 0));

                figurePrefab = figure25Prefab;

                break;
            case 26:
                figureMatrix.Add(new Vector2Int(0, 0));
                figureMatrix.Add(new Vector2Int(0, -1));
                figureMatrix.Add(new Vector2Int(-1, 0));
                figureMatrix.Add(new Vector2Int(1, -1));

                figurePrefab = figure26Prefab;

                break;
            case 27:
                figureMatrix.Add(new Vector2Int(0, 0));
                figureMatrix.Add(new Vector2Int(-1, 0));
                figureMatrix.Add(new Vector2Int(-1, 1));
                figureMatrix.Add(new Vector2Int(0, -1));

                figurePrefab = figure27Prefab;

                break;
            case 28:
                figureMatrix.Add(new Vector2Int(0, 0));
                figureMatrix.Add(new Vector2Int(-1, 0));
                figureMatrix.Add(new Vector2Int(-1, -1));
                figureMatrix.Add(new Vector2Int(0, 1));

                figurePrefab = figure28Prefab;

                break;
            case 29:
                figureMatrix.Add(new Vector2Int(0, 0));
                figureMatrix.Add(new Vector2Int(0, 1));
                figureMatrix.Add(new Vector2Int(1, 0));

                figurePrefab = figure29Prefab;

                break;
            case 30:
                figureMatrix.Add(new Vector2Int(0, 0));
                figureMatrix.Add(new Vector2Int(1, 1));
                figureMatrix.Add(new Vector2Int(1, 0));

                figurePrefab = figure30Prefab;

                break;
            case 31:
                figureMatrix.Add(new Vector2Int(0, 0));
                figureMatrix.Add(new Vector2Int(0, -1));
                figureMatrix.Add(new Vector2Int(-1, 0));

                figurePrefab = figure31Prefab;

                break;
            case 32:
                figureMatrix.Add(new Vector2Int(0, 0));
                figureMatrix.Add(new Vector2Int(-1, 0));
                figureMatrix.Add(new Vector2Int(1, 0));

                figurePrefab = figure32Prefab;

                break;
            case 33:
                figureMatrix.Add(new Vector2Int(0, 0));
                figureMatrix.Add(new Vector2Int(0, -1));
                figureMatrix.Add(new Vector2Int(-1, 0));
                figureMatrix.Add(new Vector2Int(1, 0));

                figurePrefab = figure33Prefab;

                break;
            case 34:
                figureMatrix.Add(new Vector2Int(0, 0));
                figureMatrix.Add(new Vector2Int(0, 1));
                figureMatrix.Add(new Vector2Int(0, -1));
                figureMatrix.Add(new Vector2Int(1, 0));

                figurePrefab = figure34Prefab;

                break;
            case 35:
                figureMatrix.Add(new Vector2Int(0, 0));
                figureMatrix.Add(new Vector2Int(0, 1));
                figureMatrix.Add(new Vector2Int(0, -1));
                figureMatrix.Add(new Vector2Int(-1, 0));

                figurePrefab = figure35Prefab;

                break;
            case 36:
                figureMatrix.Add(new Vector2Int(0, 0));
                figureMatrix.Add(new Vector2Int(0, -1));
                figureMatrix.Add(new Vector2Int(0, 1));
                figureMatrix.Add(new Vector2Int(-1, 1));
                figureMatrix.Add(new Vector2Int(-2, 1));

                figurePrefab = figure36Prefab;

                break;
            case 37:
                figureMatrix.Add(new Vector2Int(0, 0));
                figureMatrix.Add(new Vector2Int(0, -1));
                figureMatrix.Add(new Vector2Int(0, 1));
                figureMatrix.Add(new Vector2Int(-1, -1));
                figureMatrix.Add(new Vector2Int(-2, -1));

                figurePrefab = figure37Prefab;

                break;
            case 38:
                figureMatrix.Add(new Vector2Int(0, 0));
                figureMatrix.Add(new Vector2Int(0, -1));
                figureMatrix.Add(new Vector2Int(0, 1));
                figureMatrix.Add(new Vector2Int(1, -1));
                figureMatrix.Add(new Vector2Int(2, -1));

                figurePrefab = figure38Prefab;

                break;
            case 39:
                figureMatrix.Add(new Vector2Int(3, 0));
                figureMatrix.Add(new Vector2Int(1, 0));
                figureMatrix.Add(new Vector2Int(2, 0));
                figureMatrix.Add(new Vector2Int(0, 0));
                figureMatrix.Add(new Vector2Int(-1, 0));

                figurePrefab = figure39Prefab;

                break;
            default:
                figureMatrix.Add(new Vector2Int(0, 0));

                figurePrefab = figure2Prefab;

                break;
        }

        int colorIndex;
        switch (figureIndex)
        {
            case 1:
                figure1 = Instantiate(figurePrefab);
                figure1.gameObject.name = "Figure1";
                // Debug.Log("figure1 = " + figurePrefab);
                figure1.transform.SetParent(transform);
                figure1.transform.Translate(new Vector3(-2, -2.5f, 0));

                figure1Matrix.Clear();
                figure1Matrix = figureMatrix;

                figure1.gameObject.GetComponent<FigureScript>().OnMDown += OnMDown;
                figure1.gameObject.GetComponent<FigureScript>().OnMDrag += OnMDrag;
                figure1.gameObject.GetComponent<FigureScript>().OnMUp += OnMUp;

                figure1Colors.Clear();
                for (int i = 0; i < figure1.gameObject.transform.childCount; i++)
                {
                    // figure1.gameObject.transform.GetChild(i).GetComponent<SpriteRenderer>().color = new Color32(30, 30, 200, 255);
                    colorIndex = Random.Range(0, 4);
                    figure1Colors.Add(colorIndex);
                    figure1.gameObject.transform.GetChild(i).GetComponent<SpriteRenderer>().color = colors[colorIndex];
                }

                break;
            case 2:
                figure2 = Instantiate(figurePrefab);
                figure2.gameObject.name = "Figure2";
                // Debug.Log("figure2 = " + figurePrefab);
                figure2.transform.SetParent(transform);
                figure2.transform.Translate(new Vector3(0, -2.5f, 0));

                figure2Matrix.Clear();
                figure2Matrix = figureMatrix;

                figure2.gameObject.GetComponent<FigureScript>().OnMDown += OnMDown;
                figure2.gameObject.GetComponent<FigureScript>().OnMDrag += OnMDrag;
                figure2.gameObject.GetComponent<FigureScript>().OnMUp += OnMUp;

                figure2Colors.Clear();
                for (int i = 0; i < figure2.gameObject.transform.childCount; i++)
                {
                    // figure2.gameObject.transform.GetChild(i).GetComponent<SpriteRenderer>().color = new Color32(30, 30, 200, 255);
                    colorIndex = Random.Range(0, 4);
                    figure2Colors.Add(colorIndex);
                    figure2.gameObject.transform.GetChild(i).GetComponent<SpriteRenderer>().color = colors[colorIndex];
                }

                break;
            case 3:
                figure3 = Instantiate(figurePrefab);
                figure3.gameObject.name = "Figure3";
                // Debug.Log("figure3 = " + figurePrefab);
                figure3.transform.SetParent(transform);
                figure3.transform.Translate(new Vector3(2, -2.5f, 0));

                figure3Matrix.Clear();
                figure3Matrix = figureMatrix;

                figure3.gameObject.GetComponent<FigureScript>().OnMDown += OnMDown;
                figure3.gameObject.GetComponent<FigureScript>().OnMDrag += OnMDrag;
                figure3.gameObject.GetComponent<FigureScript>().OnMUp += OnMUp;

                figure3Colors.Clear();
                for (int i = 0; i < figure3.gameObject.transform.childCount; i++)
                {
                    // figure3.gameObject.transform.GetChild(i).GetComponent<SpriteRenderer>().color = new Color32(30, 30, 200, 255);
                    colorIndex = Random.Range(0, 4);
                    figure3Colors.Add(colorIndex);
                    figure3.gameObject.transform.GetChild(i).GetComponent<SpriteRenderer>().color = colors[colorIndex];
                }

                break;
        }
    }

    //  рисует доску
    private void BuildBoard()
    {
        //  создаем объект доски с разметкой
        board = Instantiate(boardPrefab);
        board.transform.SetParent(transform);
        //  заполняем ячейки доски клетками
        board.GetComponent<BoardScript>().Build(boardMatrice);
    }
}

// figure1.gameObject.GetComponent<FigureScript>().OnMDown += OnMDown;
// figure1.gameObject.GetComponent<FigureScript>().OnMDrag += OnMDrag;
// figure1.gameObject.GetComponent<FigureScript>().OnMUp += OnMUp;
