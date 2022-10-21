using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class MazeGenerator : TileMap
{
    const int path = 0;
    const int wall = 1;
    const int node = 0; //0 blank 1 green //2 green 3 blank
    [Export] private int width = 31;
    [Export] public int height = 38; //was originally 19, maybe do 19*3?
    [Export] private float tickDuration = 0.0f; //was 0.03f

    [Export] public int mazeOriginY = 0;
    [Export] private int maxNumMazes = 1; //maybe make public

    private Node2D gameScr;
    private bool generationComplete = false;
    //public int mazesOnScreen = 0; //have the ghost maze wall decrease this number when passing a maze chunk mazeOnScreen -= 1; //maybe make public

    //int[,] mazeArray = new int[height*maxNumMazes,width];
    static Vector2 north = new Vector2(0, -1);
    static Vector2 east = new Vector2(1, 0);
    static Vector2 south = new Vector2(0, 1);
    static Vector2 west = new Vector2(-1, 0);
    Vector2[] directions = new Vector2[] { north, east, south, west };
    List<Vector2> visited = new List<Vector2>();
    Stack<Vector2> rdfStack = new Stack<Vector2>();

    List<Vector2> wallEdgeList = new List<Vector2>();
    private int backtrackCount = 0;
    private void CorrectMazeSize()
    {
        if (width % 2 != 1)
        {
            width -= 1;
        }
        if (height % 2 != 1)
        {
            height -= 1;
        }
        GD.Print("width " + width);
        GD.Print("height " + height);
    }

    private void CreateStartingGrid()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                //wall tile edges
                if (i == 0 || i == width - 1 || j == height - 1) //j == 0
                {
                    SetCell(i, j + mazeOriginY, wall);
                    //mazeArray[j+mazeOriginY,i] = wall;

                    Vector2 wallEdge = new Vector2(i, j + mazeOriginY);
                    wallEdgeList.Add(wallEdge);
                }
                else if (j == 0)
                {
                    //SetCell(i, j + mazeOriginY, path); //TOP SHOULD BE all white, BOTTOM SHOULD BE mixed with paths. add nodes after. bottom joins to top
                }
                //alternating wall tiles
                else if (i % 2 == 0 || j % 2 == 0)
                {
                    SetCell(i, j + mazeOriginY, wall);
                    //mazeArray[j+mazeOriginY, i] = wall;
                }
                //path tiles
                else
                {
                    SetCell(i, j + mazeOriginY, path);
                    //mazeArray[j+mazeOriginY, i] = path;
                }
            }
        }
    }

    private void AddLoops(Vector2 currentV)
    {
        bool complete = false;

        for (int i = 0; i < directions.Length; i++)
        {
            if (!complete)
            {
                Vector2 newCell = new Vector2(currentV + directions[i]);
                if ((GetCellv(newCell) == wall) && (!wallEdgeList.Contains(newCell)) && (!visited.Contains(newCell)))
                {
                    SetCellv(currentV + directions[i], path);
                    //SetCellv(currentV + directions[i]*2, node); //nodeTilemap.
                    AddNode(currentV + directions[i] * 2);
                    AddNode(currentV);
                    //SetCellv(currentV,node);    //nodeTilemap.
                    complete = true;
                }
            }
        }
    }

    private void PrepMazeForJoin(int numHoles)
    {
        int oldY = mazeOriginY + height - 1;
        Random rnd = new Random();
        List<Vector2> usedCells = new List<Vector2>();

        for (int i = 1; i < width - 1; i++)
        {
            Vector2 removeCell = new Vector2(i, mazeOriginY);
            //SetCellv(removeCell+south,-1);
            //SetCellv(removeCell+south,path);
            GD.Print("remove+south: " + (removeCell + south));
            GD.Print("added path");

            GD.Print("removeCEll" + removeCell);
            if (GetCellv(removeCell + south) == wall)
            {
                SetCellv(removeCell + south, -1);
                GD.Print("deleted cell + south");
                SetCellv(removeCell + south, path);
                GD.Print("set cell+south path");

            }
        }

        while (usedCells.Count < numHoles * 3)
        {
            int cellX = rnd.Next(1, width - 1);
            Vector2 cell = new Vector2(cellX, oldY);
            if ((!usedCells.Contains(cell)))
            {
                SetCellv(cell, path);
                AddNode(cell + north);
                //SetCellv(cell+north,node); //for now, maybe replace with addnode 
                usedCells.Add(cell);
                usedCells.Add(cell + east);
                usedCells.Add(cell + west);
            }

        }
    }

    public Vector2 SetSpawn(bool spawnPacman)
    {
        int x = 0;
        int y = 0;

        Random rnd = new Random();
        if (spawnPacman)
        {
            y = height - 2;
            while ((GetCell(x, y) == wall) || (GetCell(x, y + 1) == path))
            {
                x = rnd.Next(1, width);
            }
        }
        else
        {
            while (GetCell(x, y) == wall)
            {
                x = rnd.Next(1, width);
                y = rnd.Next(1, height - 2);
            }
        }
        Vector2 spawnLoc = new Vector2(x, y);
        GD.Print("spawn" + spawnLoc); //debug

        spawnLoc = new Vector2((spawnLoc * CellSize) + (CellSize / 2));

        GD.Print("MTWspawnLoc: " + spawnLoc); //debug
        return spawnLoc;
    }

    private void AddNode(Vector2 nodeLocation)
    {
        var nodeTilemap = GetParent().GetNode<Godot.TileMap>("NodeTilemap");
        SetCellv(nodeLocation, -1); //deletes node so will remove wall node that collides
        nodeTilemap.SetCellv(nodeLocation, node); //turns it into an actual path node tile
    }

    private void rdfInit()
    {

        generationComplete = false;


        CorrectMazeSize();
        CreateStartingGrid();

        //startVector x and y must be odd, between 1+mazeOriginX/Y & height-1 / width-1 
        Vector2 startVector = new Vector2(1, mazeOriginY + 1); //Choose the initial cell,
        GD.Print("StartV: " + startVector); //debug

        visited.Add(startVector); //Mark initial cell as visited,
        rdfStack.Push(startVector); //and push it to the stack,

        rdfStep();
    }

    private void rdfStep()
    {
        Vector2 prev = new Vector2(0, 0);
        while (!generationComplete)
        {
            Vector2 curr = rdfStack.Pop(); //Pop a cell from the stack and make it a current cell.
            Vector2 next = new Vector2(0, 0);

            bool found = false;

            //check neighbours in random order //N,E,S,W walls instead of their paths, so *2
            Random rnd = new Random();
            var rndDirections = directions.OrderBy(_ => rnd.Next()).ToList(); //found this online, randomly shuffle the list.

            for (int i = 0; i < rndDirections.Count; i++)
            {
                next = 2 * rndDirections[i];
                if (GetCellv(curr + next) == path && (!visited.Contains(curr + next)))
                { //If the current cell has any neighbours which have not been visited,
                    found = true;
                    break; //Choose one of the unvisited neighbours (next),
                }
            }

            if (found)
            {
                //GD.Print("curr"+curr); //debug
                //GD.Print("prevnext "+prev); //debug
                //GD.Print("currnext "+next); //debug

                if (prev != next)
                {
                    //SetCellv(curr,node); //nodetilemap.
                    AddNode(curr);
                    //GD.Print("setcell"); //debug
                }
                prev = next;


                rdfStack.Push(curr); //Push the current cell to the stack,
                SetCellv(curr + (next / 2), path); // Remove the wall between the current cell and the chosen cell,
                visited.Add(curr + next); //Mark the chosen cell as visited,
                rdfStack.Push(curr + next); //and push it to the stack.  
                backtrackCount = 0;
            }
            else
            {
                backtrackCount++;
                if (backtrackCount == 1)
                {
                    AddLoops(curr);
                }
            }

            if (rdfStack.Count <= 0)
            { //While stack is not empty, (if stack is empty)
                AddLoops(curr);
                PrepMazeForJoin(7); //this kind of works, really i should make the bottom completely path and the top with holes in
                generationComplete = true;
                //mazesOnScreen++;
                //GD.Print("mazesOnScreen: "+mazesOnScreen); //debug

                if ((int)gameScr.Get("mazesOnTheScreen") > 0)
                {
                    //JoinMazes(7);
                    //when theres >=1 instances of the maze on the screen
                }

                GD.Print("Maze Generation Complete!"); //debug
                return;
            }
        }

    }


    //Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        gameScr = GetNode<Node2D>("/root/Game");
        mazeOriginY = (int)gameScr.Get("mazeStartLoc");
        //GD.PrintRaw("test");
        GD.Randomize();
        rdfInit();
        UpdateDirtyQuadrants();
    }
    // Called every frame. 'delta' is the elapsed time since the previous frame.

    //double timeSinceTick = 0.0;
    public override void _Process(float delta)
    {
        //GD.Print("mazeorigniy"+mazeOriginY);
        // if (mazesOnScreen < maxNumMazes)
        // {
        // 	//GD.Print("MazeOriginY: " + mazeOriginY); //debug
        // 	mazeOriginY -= height - 1;
        // 	rdfInit();
        // 	GD.Print("MazeOriginY: " + mazeOriginY); //debug
        // }
        // else if (mazesOnScreen == maxNumMazes){

        // 	//printGraph();
        // }


    }
}
