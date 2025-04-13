using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Room Level Layout", menuName = "Custom/Procedureal Generation/RoomLevelLayoutConfiguration")]
public class RoomLevelLayoutConfiguration : ScriptableObject
{
    [SerializeField] int width = 64;
    [SerializeField] int length = 64;

    [SerializeField] int roomWidthMin = 3;
    [SerializeField] int roomWidthMax = 5;
    [SerializeField] int roomLengthMin = 3;
    [SerializeField] int roomLengthMax = 5;
    [SerializeField] int doorDistanceFromEdge = 1;
    [SerializeField] int hallwayLengthMin = 3;
    [SerializeField] int hallwayLengthMax = 7;
    [SerializeField] int maxRoomCount = 10;
    [SerializeField] int minRoomDistance = 1;

    public int Width { get => width; }
    public int Length { get => length; }
    public int RoomWidthMin { get => roomWidthMin; }
    public int RoomWidthMax { get => roomWidthMax; }
    
    public int RoomLengthMin { get => roomLengthMin; }
    public int RoomLengthMax { get => roomLengthMax; }
    public int DoorDistanceFromEdge { get => doorDistanceFromEdge;}
    public int HallwayLengthMin { get => hallwayLengthMin; }
    public int HallwayLengthMax { get => hallwayLengthMax; }
    public int MaxRoomCount { get => maxRoomCount; }
    public int MinRoomDistance { get => minRoomDistance; }

}
