using System.Collections.Generic;
using UnityEngine;

public class BoardScript : MonoBehaviour
{
    // public Transform boardContainer;
    public GameObject whiteCellPrefab, greenCellPrefab, grayCellPrefab, blueCellPrefab;

    //  step = 0.575
    float[] xx = { -2.3f, -1.725f, -1.15f, -0.575f, 0f, 0.575f, 1.15f, 1.725f, 2.3f };
    float[] yy = { -0.285f, 0.29f, 0.865f, 1.44f, 2.015f, 2.59f, 3.165f, 3.74f, 4.315f };
    // Color[] colors = new Color[6] { Color.blue, Color.red, Color.green, Color.yellow, Color.magenta, Color.grey };
    Color[] colors = new Color[6] { Color.blue, Color.red, Color.yellow, Color.white, Color.green, Color.magenta };

    int[,] boardColors = new int[9, 9];
    int[,] boardFakeColors = new int[9, 9];

    void Awake()
    {
        Color blue = new Color32(50, 50, 220, 255);
        colors[0] = blue;
        Color red = new Color32(220, 50, 50, 255);
        colors[1] = red;
        Color yellow = new Color32(220, 220, 50, 255);
        colors[2] = yellow;
        Color white = new Color32(220, 220, 220, 255);
        colors[3] = white;
    }
    public void Build(GameObject[,] board)
    {
        for (int x = 0; x < 9; x++)
        {
            for (int y = 0; y < 9; y++)
            {
                if (board[x, y] == null)
                {
                    if ((x >= 3 && x <= 5 && y >= 0 && y <= 2) || (x >= 0 && x <= 2 && y >= 3 && y <= 5) || (x >= 6 && x <= 8 && y >= 3 && y <= 5) || (x >= 3 && x <= 5 && y >= 6 && y <= 8))
                    {
                        board[x, y] = Instantiate(greenCellPrefab, new Vector3(xx[x], yy[y], 0f), Quaternion.identity);
                        board[x, y].name = "GreenCell[" + x + "][" + y + "]";
                    }
                    else
                    {
                        board[x, y] = Instantiate(whiteCellPrefab, new Vector3(xx[x], yy[y], 0f), Quaternion.identity);
                        board[x, y].name = "WhiteCell[" + x + "][" + y + "]";
                    }
                    // board[x, y].transform.SetParent(boardContainer);

                    boardColors[x, y] = -1;
                }
            }
        }
    }

    public void BuildOne(GameObject[,] board, Vector2Int posInt)
    {
        int x = posInt.x;
        int y = posInt.y;

        if ((x >= 3 && x <= 5 && y >= 0 && y <= 2) || (x >= 0 && x <= 2 && y >= 3 && y <= 5) || (x >= 6 && x <= 8 && y >= 3 && y <= 5) || (x >= 3 && x <= 5 && y >= 6 && y <= 8))
        {
            board[x, y] = Instantiate(greenCellPrefab, new Vector3(xx[x], yy[y], 0f), Quaternion.identity);
            board[x, y].name = "GreenCell[" + x + "][" + y + "]";
        }
        else if ((x >= 0 && x <= 2 && y >= 0 && y <= 2) || (x >= 6 && x <= 8 && y >= 0 && y <= 2) || (x >= 3 && x <= 5 && y >= 3 && y <= 5) || (x >= 0 && x <= 2 && y >= 6 && y <= 8) || (x >= 6 && x <= 8 && y >= 6 && y <= 8))
        {
            board[x, y] = Instantiate(whiteCellPrefab, new Vector3(xx[x], yy[y], 0f), Quaternion.identity);
            board[x, y].name = "WhiteCell[" + x + "][" + y + "]";
        }
    }

    public int Exchange(GameObject[,] boardMatrice, GameObject cell, Vector2Int delta)
    {
        //  запомним первоначальное и целевое положения ячейки в матрице доски
        Vector2Int cellCoordinates = new Vector2Int(cell.GetComponent<CellScript>().coordinates.x, cell.GetComponent<CellScript>().coordinates.y);
        Vector2Int cell2Coordinates = new Vector2Int(cellCoordinates.x + delta.x, cellCoordinates.y + delta.y);

        //  получим ячейку, расположенную по целевому адресу матрицы доски
        GameObject cell2 = boardMatrice[cell2Coordinates.x, cell2Coordinates.y];
        //  если ячейка пуста
        // if (!cell2.name.Substring(0, 4).Equals("Blue"))
        //     return -1;

        //  запомним первоначальное положение ячейки в пространстве и рассчитаем новое
        Vector3 oldPosition = new Vector3(xx[cellCoordinates.x], yy[cellCoordinates.y], 0f);
        Vector3 newPosition = new Vector3(xx[cell2Coordinates.x], yy[cell2Coordinates.y], 0f);
        // Debug.Log("oldPosition = " + oldPosition + "    newPosition = " + newPosition);
        //  переместим ячейку на новое положение в пространстве
        iTween.MoveTo(cell.gameObject, iTween.Hash(
            "position", newPosition,
            "time", .3f
        ));
        iTween.MoveTo(cell2.gameObject, iTween.Hash(
            "position", oldPosition,
            "time", .3f
        ));

        //  поменяем местами объекты матрицы доски
        boardMatrice[cell2Coordinates.x, cell2Coordinates.y] = cell;
        boardMatrice[cellCoordinates.x, cellCoordinates.y] = cell2;
        boardMatrice[cell2Coordinates.x, cell2Coordinates.y].GetComponent<SpriteRenderer>().sortingOrder = 1;
        boardMatrice[cellCoordinates.x, cellCoordinates.y].GetComponent<SpriteRenderer>().sortingOrder = 0;

        int color1 = boardColors[cellCoordinates.x, cellCoordinates.y];
        int color2 = boardColors[cell2Coordinates.x, cell2Coordinates.y];
        boardColors[cell2Coordinates.x, cell2Coordinates.y] = color1;
        boardColors[cellCoordinates.x, cellCoordinates.y] = color2;

        //  установим новое положение ячейки в матрице доски
        cell.GetComponent<CellScript>().coordinates = cell2Coordinates;
        cell2.GetComponent<CellScript>().coordinates = cellCoordinates;
        SetCellCoordinates(boardMatrice);

        return 1;
    }

    public void DrawShadow(GameObject[,] board, Vector2Int posInt, List<Vector2Int> matrix)
    {
        foreach (Vector2Int matrixPos in matrix)
        {
            board[posInt.x + matrixPos.x, posInt.y + matrixPos.y] = Instantiate(grayCellPrefab, new Vector3(xx[posInt.x + matrixPos.x], yy[posInt.y + matrixPos.y], 0f), Quaternion.identity);
            board[posInt.x + matrixPos.x, posInt.y + matrixPos.y].name = "GrayCell[" + (posInt.x + matrixPos.x) + "][" + (posInt.y + matrixPos.y) + "]";
        }
    }

    public void ClearShadows(GameObject[,] board, Vector2Int posInt, bool clearAll)
    {
        //  перебираем элементы игровой доски
        //  сначала по оси X
        for (int x = 0; x < 9; x++)
        {
            // затем по оси Y
            for (int y = 0; y < 9; y++)
            {
                //  если ячейка содержит префаб серой клетки (тени)
                if (board[x, y] != null && board[x, y].gameObject.name.Substring(0, 4).Equals("Gray"))
                {
                    //  пропускаем точку центра активной фигуры, если параметр clearAll не установлен в true
                    if (x != posInt.x || y != posInt.y && !clearAll)
                    {
                        //  уничтожаем префаб тени в ячейке
                        Destroy(board[x, y].gameObject);
                        //  восстанавливаем оригинальные ячейки доски - сначала зеленые
                        if ((x >= 3 && x <= 5 && y >= 0 && y <= 2) || (x >= 0 && x <= 2 && y >= 3 && y <= 5) || (x >= 6 && x <= 8 && y >= 3 && y <= 5) || (x >= 3 && x <= 5 && y >= 6 && y <= 8))
                        {
                            board[x, y] = Instantiate(greenCellPrefab, new Vector3(xx[x], yy[y], 0f), Quaternion.identity);
                            board[x, y].name = "GreenCell[" + x + "][" + y + "]";
                        }
                        //  затем белые
                        else
                        {
                            board[x, y] = Instantiate(whiteCellPrefab, new Vector3(xx[x], yy[y], 0f), Quaternion.identity);
                            board[x, y].name = "WhiteCell[" + x + "][" + y + "]";
                        }
                    }
                    //  если параметр clearAll установлен в true, либо координаты ячейки отличаются от координат центра активной фигуры
                    else
                    {
                        //  уничтожаем префаб, содержащийся в ячеке
                        Destroy(board[x, y].gameObject);
                        //  восстанавливаем оригинальные ячейки доски - сначала зеленые
                        if ((x >= 3 && x <= 5 && y >= 0 && y <= 2) || (x >= 0 && x <= 2 && y >= 3 && y <= 5) || (x >= 6 && x <= 8 && y >= 3 && y <= 5) || (x >= 3 && x <= 5 && y >= 6 && y <= 8))
                        {
                            board[x, y] = Instantiate(greenCellPrefab, new Vector3(xx[x], yy[y], 0f), Quaternion.identity);
                            board[x, y].name = "GreenCell[" + x + "][" + y + "]";
                        }
                        //  затем белые
                        else
                        {
                            board[x, y] = Instantiate(whiteCellPrefab, new Vector3(xx[x], yy[y], 0f), Quaternion.identity);
                            board[x, y].name = "WhiteCell[" + x + "][" + y + "]";
                        }
                    }
                }
            }
        }
    }

    public void ClearOneShadow(GameObject[,] board, Vector2Int posInt)
    {
        int x = posInt.x;
        int y = posInt.y;

        Destroy(board[x, y].gameObject);
        if ((x >= 3 && x <= 5 && y >= 0 && y <= 2) || (x >= 0 && x <= 2 && y >= 3 && y <= 5) || (x >= 6 && x <= 8 && y >= 3 && y <= 5) || (x >= 3 && x <= 5 && y >= 6 && y <= 8))
        {
            board[x, y] = Instantiate(greenCellPrefab, new Vector3(xx[x], yy[y], 0f), Quaternion.identity);
            board[x, y].name = "GreenCell[" + x + "][" + y + "]";
        }
        else
        {
            board[x, y] = Instantiate(whiteCellPrefab, new Vector3(xx[x], yy[y], 0f), Quaternion.identity);
            board[x, y].name = "WhiteCell[" + x + "][" + y + "]";
        }
    }

    public void DrawFigure(GameObject[,] board, Vector2Int posInt, GameObject figure)
    {
        int x = posInt.x;
        int y = posInt.y;

        Destroy(board[x, y].gameObject);
        board[x, y] = Instantiate(blueCellPrefab, new Vector3(xx[x], yy[y], 0f), Quaternion.identity);
        board[x, y].name = "BlueCell[" + x + "][" + y + "]";
    }

    public void DrawFigureInsteadOfShadow(GameObject[,] board, Vector2Int posInt, List<Vector2Int> matrix, List<int> figureColors)
    {
        int i = 0;
        foreach (Vector2Int matrixPos in matrix)
        {
            Destroy(board[posInt.x + matrixPos.x, posInt.y + matrixPos.y].gameObject);
            board[posInt.x + matrixPos.x, posInt.y + matrixPos.y] = Instantiate(blueCellPrefab, new Vector3(xx[posInt.x + matrixPos.x], yy[posInt.y + matrixPos.y], 0f), Quaternion.identity);
            board[posInt.x + matrixPos.x, posInt.y + matrixPos.y].name = "BlueCell[" + (posInt.x + matrixPos.x) + "][" + (posInt.y + matrixPos.y) + "]";
            // board[posInt.x + matrixPos.x, posInt.y + matrixPos.y].gameObject.GetComponent<SpriteRenderer>().color = new Color32(50, 50, 255, 255);
            
            board[posInt.x + matrixPos.x, posInt.y + matrixPos.y].GetComponent<CellScript>().coordinates = new Vector2Int(posInt.x + matrixPos.x, posInt.y + matrixPos.y);

            board[posInt.x + matrixPos.x, posInt.y + matrixPos.y].gameObject.GetComponent<SpriteRenderer>().color = colors[figureColors[i]];
            boardColors[posInt.x + matrixPos.x, posInt.y + matrixPos.y] = figureColors[i];

            board[posInt.x + matrixPos.x, posInt.y + matrixPos.y].gameObject.GetComponent<SpriteRenderer>().sortingOrder = 0;
            i++;
        }

        // for (int i = 0; i < matrix.Count; i++)
        // {
        //     Destroy(board[posInt.x + matrix[i].x, posInt.y + matrix[i].y].gameObject);
        //     board[posInt.x + matrix[i].x, posInt.y + matrix[i].y] = Instantiate(blueCellPrefab, new Vector3(xx[posInt.x + matrix[i].x], yy[posInt.y + matrix[i].y], 0f), Quaternion.identity);
        //     board[posInt.x + matrix[i].x, posInt.y + matrix[i].y].name = "BlueCell[" + (posInt.x + matrix[i].x) + "][" + (posInt.y + matrix[i].y) + "]";
        //     // board[posInt.x + matrixPos.x, posInt.y + matrixPos.y].gameObject.GetComponent<SpriteRenderer>().color = new Color32(50, 50, 255, 255);
        //     board[posInt.x + matrix[i].x, posInt.y + matrix[i].y].gameObject.GetComponent<SpriteRenderer>().color = colors[figureColors[i]];
        //     board[posInt.x + matrix[i].x, posInt.y + matrix[i].y].gameObject.GetComponent<SpriteRenderer>().sortingOrder = 0;
        // }
    }

    public void DrawFakeFigureInsteadOfShadow(Vector2Int posInt, List<Vector2Int> matrix, List<int> figureColors)
    {
        for (int x = 0; x < 9; x++)
            for (int y = 0; y < 9; y++)
                boardFakeColors[x, y] = boardColors[x, y];

        int i = 0;
        foreach (Vector2Int matrixPos in matrix)
        {
            boardFakeColors[posInt.x + matrixPos.x, posInt.y + matrixPos.y] = figureColors[i];
            i++;
        }
    }

    public bool CheckSwapWinners(int X, int Y, List<Vector2Int> neighbours)
    {
        foreach (Vector2Int neighbour in neighbours)
        {
            for (int x = 0; x < 9; x++)
                for (int y = 0; y < 9; y++)
                    boardFakeColors[x, y] = boardColors[x, y];

            int neighbourColor = boardFakeColors[neighbour.x, neighbour.y];
            // if (neighbourColor == -1)
            //     continue;
            boardFakeColors[neighbour.x, neighbour.y] = boardFakeColors[X, Y];
            boardFakeColors[X, Y] = neighbourColor;

            int col;
            List<Vector2Int> sameColorCells = new List<Vector2Int>();
            // List<Vector2Int> winners = new List<Vector2Int>();
            // проверим цвета по вертикали
            //  переберем каждую колонку
            for (int x = 0; x < 9; x++)
            {
                sameColorCells.Clear();
                //  ищем совпадения в очередной колонке
                for (int y = 0; y < 9; y++)
                {
                    //  считываем цвет
                    col = boardFakeColors[x, y];

                    //  если пока совпадений не было найдено
                    if (sameColorCells.Count == 0)
                    {
                        //  если ячейка не пуста
                        if (col != -1)
                            //  добавляем координату ячейки в список совпадений
                            sameColorCells.Add(new Vector2Int(x, y));
                    }
                    //  если список совпадений содержит хотя бы одну ячейку
                    else
                    {
                        //  если цвет текущей ячейки равен цвету предыдущей ячейки
                        if (boardFakeColors[x, y - 1] == col)
                        {
                            //  добавляем координату ячейки в список совпадений
                            sameColorCells.Add(new Vector2Int(x, y));
                        }
                        //  иначе если цвет текущей ячейки отличается от цвета предыдущей либо текущая ячейка пуста
                        else
                        {
                            //  если в списке совпадений больше двух позиций
                            if (sameColorCells.Count >= 3)
                                //  есть swap
                                return true;
                            //  очищаем список совпадений
                            sameColorCells.Clear();
                            //  если текущая ячейка не пуста
                            if (col != -1)
                                //  добавляем координату ячейки в список совпадений
                                sameColorCells.Add(new Vector2Int(x, y));
                        }
                    }
                }
                //  если в списке совпадений больше двух позиций
                if (sameColorCells.Count >= 3)
                    //  есть swap
                    return true;
                sameColorCells.Clear();
            }

            // проверим цвета по горизонтали
            //  переберем каждую строку
            for (int y = 0; y < 9; y++)
            {
                sameColorCells.Clear();
                //  ищем совпадения в очередной строке
                for (int x = 0; x < 9; x++)
                {
                    //  считываем цвет
                    col = boardFakeColors[x, y];

                    //  если пока совпадений не было найдено
                    if (sameColorCells.Count == 0)
                    {
                        //  если ячейка не пуста
                        if (col != -1)
                            //  добавляем координату ячейки в список совпадений
                            sameColorCells.Add(new Vector2Int(x, y));
                    }
                    //  если список совпадений содержит хотя бы одну ячейку
                    else
                    {
                        //  если цвет текущей ячейки равен цвету предыдущей ячейки
                        if (boardFakeColors[x - 1, y] == col)
                        {
                            //  добавляем координату ячейки в список совпадений
                            sameColorCells.Add(new Vector2Int(x, y));
                        }
                        //  иначе если цвет текущей ячейки отличается от цвета предыдущей либо текущая ячейка пуста
                        else
                        {
                            //  если в списке совпадений больше двух позиций
                            if (sameColorCells.Count >= 3)
                                //  есть swap
                                return true;
                            //  очищаем список совпадений
                            sameColorCells.Clear();
                            //  если текущая ячейка не пуста
                            if (col != -1)
                                //  добавляем координату ячейки в список совпадений
                                sameColorCells.Add(new Vector2Int(x, y));
                        }
                    }
                }
                //  если в списке совпадений больше двух позиций
                if (sameColorCells.Count >= 3)
                    //  есть swap
                    return true;
                sameColorCells.Clear();
            }
        }
        return false;
    }

    public Vector2Int GetClosest(GameObject figure, Vector2 pos)
    {
        float minX = 100, minY = 100;
        int xIndex = 100, yIndex = 100;
        for (int x = 0; x < 9; x++)
        {
            if (Mathf.Abs(pos.x - xx[x]) < minX)
            {
                minX = Mathf.Abs(pos.x - xx[x]);
                xIndex = x;
            }
            for (int y = 0; y < 9; y++)
            {
                if (Mathf.Abs(pos.y - yy[y]) < minY)
                {
                    minY = Mathf.Abs(pos.y - yy[y]);
                    yIndex = y;
                }
            }
        }
        Vector2Int closest = new Vector2Int(xIndex, yIndex);
        if (minX > 0.3 || minY > 0.3)
        {
            closest.x = 100;
            closest.y = 100;
        }
        return closest;
    }

    public int SeekWinnersAndDestroy(GameObject[,] board)
    {
        List<Vector2Int> winners = new List<Vector2Int>();
        int scorePoints = 0;

        //  проверим заполненность по вертикали
        for (int x = 0; x < 9; x++)
        {
            bool verticalWins = true;
            for (int y = 0; y < 9; y++)
            {
                if (board[x, y] == null)
                    print("board[x, y] == null");
                else
                    if (!board[x, y].gameObject.name.Substring(0, 4).Equals("Blue"))
                    {
                        verticalWins = false;
                        break;
                    }
            }
            if (verticalWins)
            {
                for (int y = 0; y < 9; y++)
                {
                    if (!winners.Contains(new Vector2Int(x, y)))
                        winners.Add(new Vector2Int(x, y));
                }
            }
        }

        //  проверим заполненность по горизонтали
        for (int y = 0; y < 9; y++)
        {
            bool horizontalWins = true;
            for (int x = 0; x < 9; x++)
            {
                if (board[x, y] == null)
                    print("board[x, y] == null");
                else
                    if (!board[x, y].gameObject.name.Substring(0, 4).Equals("Blue"))
                    {
                        horizontalWins = false;
                        break;
                    }
            }
            if (horizontalWins)
            {
                for (int x = 0; x < 9; x++)
                {
                    if (!winners.Contains(new Vector2Int(x, y)))
                        winners.Add(new Vector2Int(x, y));
                }
            }
        }

        //  проверим заполненность блоков
        for (int X = 0; X < 3; X++)
        {
            for (int Y = 0; Y < 3; Y++)
            {
                bool blockWins = true;

                for (int x = X * 3; x < X * 3 + 3; x++)
                {
                    for (int y = Y * 3; y < Y * 3 + 3; y++)
                    {
                        if (board[x, y] == null)
                            print("board[x, y] == null");
                        else
                            if (!board[x, y].gameObject.name.Substring(0, 4).Equals("Blue"))
                            {
                                blockWins = false;
                                break;
                            }
                    }
                    if (!blockWins)
                        break;
                }
                if (blockWins)
                {
                    for (int x = X * 3; x < X * 3 + 3; x++)
                    {
                        for (int y = Y * 3; y < Y * 3 + 3; y++)
                        {
                            if (!winners.Contains(new Vector2Int(x, y)))
                                winners.Add(new Vector2Int(x, y));
                        }
                    }
                }
            }
        }

        int col;
        List<Vector2Int> sameColorCells = new List<Vector2Int>();
        // проверим цвета по вертикали
        //  переберем каждую колонку
        for (int x = 0; x < 9; x++)
        {
            sameColorCells.Clear();
            //  ищем совпадения в очередной колонке
            for (int y = 0; y < 9; y++)
            {
                //  считываем цвет
                col = boardColors[x, y];

                //  если пока совпадений не было найдено
                if (sameColorCells.Count == 0)
                {
                    //  если ячейка не пуста
                    if (col != -1)
                        //  добавляем координату ячейки в список совпадений
                        sameColorCells.Add(new Vector2Int(x, y));
                }
                //  если список совпадений содержит хотя бы одну ячейку
                else
                {
                    //  если цвет текущей ячейки равен цвету предыдущей ячейки
                    if (boardColors[x, y - 1] == col)
                    {
                        //  добавляем координату ячейки в список совпадений
                        sameColorCells.Add(new Vector2Int(x, y));
                    }
                    //  иначе если цвет текущей ячейки отличается от цвета предыдущей либо текущая ячейка пуста
                    else
                    {
                        //  если в списке совпадений больше двух позиций
                        if (sameColorCells.Count >= 3)
                            //  добавим список совпадений в выигрышный список
                            winners.AddRange(sameColorCells);
                        //  очищаем список совпадений
                        sameColorCells.Clear();
                        //  если текущая ячейка не пуста
                        if (col != -1)
                            //  добавляем координату ячейки в список совпадений
                            sameColorCells.Add(new Vector2Int(x, y));
                    }
                }
            }
            //  если в списке совпадений больше двух позиций
            if (sameColorCells.Count >= 3)
                //  добавим список совпадений в выигрышный список
                winners.AddRange(sameColorCells);
            sameColorCells.Clear();
        }

        // проверим цвета по горизонтали
        //  переберем каждую строку
        for (int y = 0; y < 9; y++)
        {
            sameColorCells.Clear();
            //  ищем совпадения в очередной строке
            for (int x = 0; x < 9; x++)
            {
                //  считываем цвет
                col = boardColors[x, y];

                //  если пока совпадений не было найдено
                if (sameColorCells.Count == 0)
                {
                    //  если ячейка не пуста
                    if (col != -1)
                        //  добавляем координату ячейки в список совпадений
                        sameColorCells.Add(new Vector2Int(x, y));
                }
                //  если список совпадений содержит хотя бы одну ячейку
                else
                {
                    //  если цвет текущей ячейки равен цвету предыдущей ячейки
                    if (boardColors[x - 1, y] == col)
                    {
                        //  добавляем координату ячейки в список совпадений
                        sameColorCells.Add(new Vector2Int(x, y));
                    }
                    //  иначе если цвет текущей ячейки отличается от цвета предыдущей либо текущая ячейка пуста
                    else
                    {
                        //  если в списке совпадений больше двух позиций
                        if (sameColorCells.Count >= 3)
                            //  добавим список совпадений в выигрышный список
                            winners.AddRange(sameColorCells);
                        //  очищаем список совпадений
                        sameColorCells.Clear();
                        //  если текущая ячейка не пуста
                        if (col != -1)
                            //  добавляем координату ячейки в список совпадений
                            sameColorCells.Add(new Vector2Int(x, y));
                    }
                }
            }
            //  если в списке совпадений больше двух позиций
            if (sameColorCells.Count >= 3)
                //  добавим список совпадений в выигрышный список
                winners.AddRange(sameColorCells);
            sameColorCells.Clear();
        }

        Vector2 iTweenPos;
        foreach (Vector2Int winner in winners)
        {
            board[winner.x, winner.y].GetComponent<SpriteRenderer>().sortingOrder = 10;
            iTweenPos.x = Random.Range(-4f, 4f);
            iTweenPos.y = Random.Range(-5f, 5f);

            iTween.MoveBy(board[winner.x, winner.y].gameObject, iTween.Hash(
                "x", iTweenPos.x,
                "y", iTweenPos.y,
                "time", 1.5f));
            iTween.ScaleTo(board[winner.x, winner.y].gameObject, Vector3.zero, 1.5f);
            iTween.FadeTo(board[winner.x, winner.y].gameObject, 10f, 1.5f);
            Destroy(board[winner.x, winner.y].gameObject, 1.5f);
            BuildOne(board, winner);
            scorePoints++;

            boardColors[winner.x, winner.y] = -1;
            board[winner.x, winner.y].GetComponent<SpriteRenderer>().sortingOrder = 0;
        }

        return scorePoints;
    }

    public int SeekWinnersAndHighlight(GameObject[,] board)
    {
        List<Vector2Int> winners = new List<Vector2Int>();
        int scorePoints = 0;

        //  проверим заполненность по вертикали
        for (int x = 0; x < 9; x++)
        {
            bool verticalWins = true;
            for (int y = 0; y < 9; y++)
            {
                if (boardFakeColors[x, y] == -1)
                {
                    verticalWins = false;
                    break;
                }
            }
            if (verticalWins)
            {
                for (int y = 0; y < 9; y++)
                {
                    if (!winners.Contains(new Vector2Int(x, y)))
                        winners.Add(new Vector2Int(x, y));
                }
            }
        }

        //  проверим заполненность по горизонтали
        for (int y = 0; y < 9; y++)
        {
            bool horizontalWins = true;
            for (int x = 0; x < 9; x++)
            {
                if (boardFakeColors[x, y] == -1)
                {
                    horizontalWins = false;
                    break;
                }
            }
            if (horizontalWins)
            {
                for (int x = 0; x < 9; x++)
                {
                    if (!winners.Contains(new Vector2Int(x, y)))
                        winners.Add(new Vector2Int(x, y));
                }
            }
        }

        //  проверим заполненность блоков
        for (int X = 0; X < 3; X++)
        {
            for (int Y = 0; Y < 3; Y++)
            {
                bool blockWins = true;

                for (int x = X * 3; x < X * 3 + 3; x++)
                {
                    for (int y = Y * 3; y < Y * 3 + 3; y++)
                    {
                        if (boardFakeColors[x, y] == -1)
                        {
                            blockWins = false;
                            break;
                        }
                    }
                    if (!blockWins)
                        break;
                }
                if (blockWins)
                {
                    for (int x = X * 3; x < X * 3 + 3; x++)
                    {
                        for (int y = Y * 3; y < Y * 3 + 3; y++)
                        {
                            if (!winners.Contains(new Vector2Int(x, y)))
                                winners.Add(new Vector2Int(x, y));
                        }
                    }
                }
            }
        }

        int col;
        List<Vector2Int> sameColorCells = new List<Vector2Int>();
        // проверим цвета по вертикали
        //  переберем каждую колонку
        for (int x = 0; x < 9; x++)
        {
            sameColorCells.Clear();
            //  ищем совпадения в очередной колонке
            for (int y = 0; y < 9; y++)
            {
                //  считываем цвет
                col = boardFakeColors[x, y];

                //  если пока совпадений не было найдено
                if (sameColorCells.Count == 0)
                {
                    //  если ячейка не пуста
                    if (col != -1)
                        //  добавляем координату ячейки в список совпадений
                        sameColorCells.Add(new Vector2Int(x, y));
                }
                //  если список совпадений содержит хотя бы одну ячейку
                else
                {
                    //  если цвет текущей ячейки равен цвету предыдущей ячейки
                    if (boardFakeColors[x, y - 1] == col)
                    {
                        //  добавляем координату ячейки в список совпадений
                        sameColorCells.Add(new Vector2Int(x, y));
                    }
                    //  иначе если цвет текущей ячейки отличается от цвета предыдущей либо текущая ячейка пуста
                    else
                    {
                        //  если в списке совпадений больше двух позиций
                        if (sameColorCells.Count >= 3)
                            //  добавим список совпадений в выигрышный список
                            winners.AddRange(sameColorCells);
                        //  очищаем список совпадений
                        sameColorCells.Clear();
                        //  если текущая ячейка не пуста
                        if (col != -1)
                            //  добавляем координату ячейки в список совпадений
                            sameColorCells.Add(new Vector2Int(x, y));
                    }
                }
            }
            //  если в списке совпадений больше двух позиций
            if (sameColorCells.Count >= 3)
                //  добавим список совпадений в выигрышный список
                winners.AddRange(sameColorCells);
            sameColorCells.Clear();
        }

        // проверим цвета по горизонтали
        //  переберем каждую строку
        for (int y = 0; y < 9; y++)
        {
            sameColorCells.Clear();
            //  ищем совпадения в очередной строке
            for (int x = 0; x < 9; x++)
            {
                //  считываем цвет
                col = boardFakeColors[x, y];

                //  если пока совпадений не было найдено
                if (sameColorCells.Count == 0)
                {
                    //  если ячейка не пуста
                    if (col != -1)
                        //  добавляем координату ячейки в список совпадений
                        sameColorCells.Add(new Vector2Int(x, y));
                }
                //  если список совпадений содержит хотя бы одну ячейку
                else
                {
                    //  если цвет текущей ячейки равен цвету предыдущей ячейки
                    if (boardFakeColors[x - 1, y] == col)
                    {
                        //  добавляем координату ячейки в список совпадений
                        sameColorCells.Add(new Vector2Int(x, y));
                    }
                    //  иначе если цвет текущей ячейки отличается от цвета предыдущей либо текущая ячейка пуста
                    else
                    {
                        //  если в списке совпадений больше двух позиций
                        if (sameColorCells.Count >= 3)
                            //  добавим список совпадений в выигрышный список
                            winners.AddRange(sameColorCells);
                        //  очищаем список совпадений
                        sameColorCells.Clear();
                        //  если текущая ячейка не пуста
                        if (col != -1)
                            //  добавляем координату ячейки в список совпадений
                            sameColorCells.Add(new Vector2Int(x, y));
                    }
                }
            }
            //  если в списке совпадений больше двух позиций
            if (sameColorCells.Count >= 3)
                //  добавим список совпадений в выигрышный список
                winners.AddRange(sameColorCells);
            sameColorCells.Clear();
        }

        foreach (Vector2Int winner in winners)
        {
            if (boardColors[winner.x, winner.y] != -1)
                board[winner.x, winner.y].transform.GetComponent<SpriteRenderer>().color = colors[boardFakeColors[winner.x, winner.y]] * 1.6f;
        }

        return scorePoints;
    }

    public void ClearHighlights(GameObject[,] board)
    {
        for (int y = 0; y < 9; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                //  этот фрагмент нужен если мышкой удаляем клетки, т.к. во время удаления содержимое boardColors не обновляется
                // if (board[x, y] == null)
                //     boardColors[x, y] = -1;
                if (boardColors[x, y] != -1)
                    board[x, y].transform.GetComponent<SpriteRenderer>().color = colors[boardColors[x, y]];
            }
        }
    }

    public void SetCellCoordinates(GameObject[,] board)
    {
        //  переберем матрицу объектов доски
        for (int x = 0; x < 9; x++)
        {
            for (int y = 0; y < 9; y++)
            {
                //  если объект матрицы равен null
                if (board[x, y] == null)
                {
                    print("board[x, y] == null");
                    //  создадим пустой объект
                    BuildOne(board, new Vector2Int(x, y));
                    board[x, y].GetComponent<CellScript>().coordinates = new Vector2Int(x, y);
                    boardColors[x, y] = -1;
                    board[x, y].name = "White";
                }
                //  объект не равен null, а его имя равно "Blue"
                else if (board[x, y].name.Substring(0, 4) == "Blue")
                {
                    board[x, y].GetComponent<CellScript>().coordinates = new Vector2Int(x, y);
                    // boardColors[x, y] = -1;
                }
                //  объект не равен null, и его имя не равно "Blue"
                else
                {
                    board[x, y].GetComponent<CellScript>().coordinates = new Vector2Int(x, y);
                    boardColors[x, y] = -1;
                }
            }
        }
    }
}
