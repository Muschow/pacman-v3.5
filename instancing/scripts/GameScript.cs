using Godot;
using System;

public class GameScript : Node2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    private TileMap mazeTm;
    private KinematicBody2D pacman;
    private int mazeStartLoc = 0;
    private int mazeHeight;
    private int oldMazeY;
    private int mazesOnTheScreen = 0;
    PackedScene mazeScene = GD.Load<PackedScene>("res://scenes/Maze.tscn");

    // Called when the node enters the scene tree for the first time.


    public override void _Ready()
    {
        mazeTm = GetNode<TileMap>("/root/Game/Maze/MazeTilemap");
        mazeHeight = (int)mazeTm.Get("height");
        mazeStartLoc = (int)mazeTm.Get("mazeOriginY");
        oldMazeY = mazeHeight * (-2);
        GD.Print("oldMazeY ready" + oldMazeY);
        mazesOnTheScreen++; //remove this if you instance the first maze

        pacman = GetNode<KinematicBody2D>("/root/Game/Pacman"); // res://scenes/Pacman.tscn
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        //GD.Print("mazeStartLoc"+mazeStartLoc);
        //GD.Print("start+height-1 "+mazeStartLoc+18);
        GD.Print("playerPos " + Math.Floor(pacman.Position.y / 32));
        //PrintTreePretty();

        if (Math.Floor(pacman.Position.y / 32) == mazeStartLoc + mazeHeight - 2)
        {
            Node mazeInstance = mazeScene.Instance();
            mazeStartLoc -= (mazeHeight - 1);
            AddChild(mazeInstance, true);
            mazesOnTheScreen++;
            GD.Print("instanced!");
            //instance maze tscn
            //joinMazes where oldY = mazeoriginy+height-1 * 3??? not sure
        }

        if (Math.Floor(pacman.Position.y / 32) == oldMazeY + 5)
        {
            //delete old maze chunk
            oldMazeY -= mazeHeight;
            //remove child maze and then on exit tree queue free and see what happens
            //QueueFree();
            GD.Print("oldMazeY new" + oldMazeY);
        }

    }
}
