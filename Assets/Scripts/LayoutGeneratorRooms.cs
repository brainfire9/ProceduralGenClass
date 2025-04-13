using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Unity.VisualScripting;
using Random = System.Random;

public class NewBehaviourScript : MonoBehaviour
{
    [SerializeField] int seed = Environment.TickCount;
    [SerializeField] RoomLevelLayoutConfiguration levelConfig;

    [SerializeField] GameObject levelLayoutDisplay;
    [SerializeField] List<Hallway> openDoorways;
    
    Random random;
    Level level;

    [ContextMenu("Generate Level Layout")]
    public void GenerateLevel() {
        random = new Random(seed);
        openDoorways = new List<Hallway>();
        level = new Level(levelConfig.Width, levelConfig.Length);
        var roomRect = GetStartRoomRect();
        Debug.Log(roomRect);
        Room room = new Room(roomRect);
        List<Hallway> hallways = room.CalculateAllPossibleDoorways(room.Area.width, room.Area.height, levelConfig.DoorDistanceFromEdge);
        hallways.ForEach(h => h.StartRoom = room);
        hallways.ForEach(h => openDoorways.Add(h));
        level.AddRoom(room);

        Hallway selectedEntryway = openDoorways[random.Next(openDoorways.Count)];
        AddRooms();
        DrawLayout(selectedEntryway, roomRect);
    }

    [ContextMenu("Generate New Seed")]
    public void GenerateNewSeed() {
        seed = Environment.TickCount;

    }

    [ContextMenu("Generate New Seed And Level")]
    public void GenerateNewSeedAndLeve() {
        GenerateNewSeed();
        GenerateLevel();
    }
    RectInt GetStartRoomRect() {
        int roomWidth = random.Next(levelConfig.RoomWidthMin, levelConfig.RoomWidthMax);
        int availableWidthX = levelConfig.Width / 2 - roomWidth;
        int randomX = random.Next(0, availableWidthX);
        int roomX = randomX - (levelConfig.Width/4);

        int roomLength = random.Next(levelConfig.RoomLengthMin, levelConfig.RoomLengthMax);
        int availableLengthY = levelConfig.Length / 2 - roomLength;
        int randomY = random.Next(0, availableLengthY);
        int roomY = randomY + (levelConfig.Length / 4);

        return new RectInt(roomX, roomY, roomWidth, roomLength);
    }

    void DrawLayout(Hallway selectedEntryway = null, RectInt roomCandidateRect = new RectInt(), bool isDebug = false ) 
    {
        var renderer = levelLayoutDisplay.GetComponent<Renderer>();
        
        var layoutTexture = (Texture2D) renderer.sharedMaterial.mainTexture;

        layoutTexture.Reinitialize(levelConfig.Width, levelConfig.Length);
        levelLayoutDisplay.transform.localScale = new Vector3(levelConfig.Width, levelConfig.Length, 1);
        layoutTexture.FillWithColor(Color.black);

        Array.ForEach(level.Rooms, room => layoutTexture.DrawRectangle(room.Area, Color.white)) ;
        Array.ForEach(level.Hallways, hallway => layoutTexture.DrawLine(hallway.StartPositionAbsolute,hallway.EndPositionAbsolute, Color.white)  );

        if (isDebug) {
            layoutTexture.DrawRectangle(roomCandidateRect, Color.blue);
            openDoorways.ForEach(hallway => layoutTexture.SetPixel(hallway.StartPositionAbsolute.x, hallway.StartPositionAbsolute.y, hallway.StartDirection.GetColor()));
        }
        
        
        if (isDebug && selectedEntryway != null) {
            layoutTexture.SetPixel(selectedEntryway.StartPositionAbsolute.x, selectedEntryway.StartPositionAbsolute.y, Color.red);
        }

        layoutTexture.SaveAsset();
    }

    private Hallway SelectHallwayCandidate(RectInt roomCandidateRect, Hallway entryWay)
    {
        Room room = new Room(roomCandidateRect);
        List<Hallway> candidates = room.CalculateAllPossibleDoorways(room.Area.width, room.Area.height, levelConfig.DoorDistanceFromEdge);
        HallwayDirection requiredDirection = entryWay.StartDirection.GetOppositeDirection();
        List<Hallway> filteredHallwayCondidates = candidates.Where(hallwayCandidate => hallwayCandidate.StartDirection == requiredDirection).ToList();
        return filteredHallwayCondidates.Count > 0 ? filteredHallwayCondidates[random.Next(filteredHallwayCondidates.Count)] : null;
    }

    Vector2Int CalculateRoomPosition(Hallway entryway, int roomWidth, int roomLength, int distance, Vector2Int endPosition)
    {
        Vector2Int roomPosition = entryway.StartPositionAbsolute;
        switch (entryway.StartDirection) 
        {
            case HallwayDirection.Left:
                roomPosition.x -= distance + roomWidth;
                roomPosition.y -= endPosition.y;
                break;
            case HallwayDirection.Top:
                roomPosition.x -= endPosition.x;
                roomPosition.y += distance + 1;
                break;
            case HallwayDirection.Right:
                roomPosition.x += distance + 1;
                roomPosition.y -= endPosition.y;
                break;
            case HallwayDirection.Bottom:
                roomPosition.x -= endPosition.x;
                roomPosition.y -= distance + roomLength;
                break;
        }
        return roomPosition;
    }

    private Room ConstructAdjacentRoom(Hallway selectedEntryway)
    {
        RectInt roomCandidateRect = new RectInt 
        {
            width = random.Next(levelConfig.RoomWidthMin, levelConfig.RoomWidthMax),
            height = random.Next(levelConfig.RoomLengthMin, levelConfig.RoomLengthMax)
        };

        Hallway selectedExit = SelectHallwayCandidate(roomCandidateRect, selectedEntryway);
        if (selectedExit == null) 
        {
            return null;
        }
        
        int hallwayLength = random.Next(levelConfig.HallwayLengthMin, levelConfig.HallwayLengthMax + 1 );
        Vector2Int roomCandidatePosition = CalculateRoomPosition(selectedEntryway, roomCandidateRect.width, roomCandidateRect.height, hallwayLength, selectedExit.StartPosition);
        roomCandidateRect.position = roomCandidatePosition;

        if (!IsRoomCandidateValid(roomCandidateRect))
        {
            return null;
        }

        Room newroom = new Room(roomCandidateRect);
        selectedEntryway.EndRoom = newroom;
        selectedEntryway.EndPosition = selectedExit.StartPosition;
        return newroom;
    }
    void AddRooms()
    {
        Debug.Log($"openDoorways.Count = {openDoorways.Count}, level.Rooms.Length = {level.Rooms.Length}");
        while (openDoorways.Count > 0 && level.Rooms.Length < levelConfig.MaxRoomCount)
        {
            Debug.Log("Adding room...");
            Hallway selectedEntryway = openDoorways[random.Next(0, openDoorways.Count)];
            Room newRoom = ConstructAdjacentRoom(selectedEntryway);

            if (newRoom == null) {
                Debug.Log("New room is null, discarding...");
                openDoorways.Remove(selectedEntryway);
                continue;
            }

            level.AddRoom(newRoom);
            level.AddHallway(selectedEntryway);

            selectedEntryway.EndRoom = newRoom;
            List<Hallway> newOpenHallways = newRoom.CalculateAllPossibleDoorways(newRoom.Area.width, newRoom.Area.height, levelConfig.DoorDistanceFromEdge);
            newOpenHallways.ForEach(hallway => hallway.StartRoom = newRoom);

            openDoorways.Remove(selectedEntryway);
            openDoorways.AddRange(newOpenHallways);
        }
    }

    private bool IsRoomCandidateValid(RectInt roomCandidateRect)
    {
        int bufferSpace = 2;
        RectInt levelRect = new RectInt(1, 1, levelConfig.Width - bufferSpace, levelConfig.Length - bufferSpace);
        Debug.Log($"contains? {levelRect.Contains(roomCandidateRect)}");
        Debug.Log($"overlap? {CheckRoomOverlap(roomCandidateRect, level.Rooms, level.Hallways, 1)}");
        return levelRect.Contains(roomCandidateRect) && !CheckRoomOverlap(roomCandidateRect, level.Rooms, level.Hallways, levelConfig.MinRoomDistance);
    } 

    private bool CheckRoomOverlap(RectInt roomCandidateRect, Room[] rooms, Hallway[] hallways, int minRoomDistance)
    {
        RectInt paddedRoomRect = new RectInt 
        {
            x = roomCandidateRect.x - minRoomDistance,
            y = roomCandidateRect.y - minRoomDistance,
            width = roomCandidateRect.width + 2 * minRoomDistance,
            height = roomCandidateRect.height + 2 * minRoomDistance
        };
        foreach (Room room in rooms)
        {
            if (paddedRoomRect.Overlaps(room.Area))
            {
                return true;
            }
        }
        foreach (Hallway hallway in hallways)
        {
            if (paddedRoomRect.Overlaps(hallway.Area))
            {
                return true;
            }
        }
        return false;
        

    }
}


