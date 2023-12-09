using System.Collections.Generic;
using UnityEngine;
using Util;

namespace FloorGen
{
    public class MinimapUI : MonoBehaviour
    {
        [SerializeField] private Vector2Int iconOffsets = new(65, 65);
        [SerializeField] private GameObject minimapIconPrefab;
        [SerializeField] private GameObject minimapBridgeIconPrefab;
        [SerializeField] private Vector2 wideBridgeSize = new(25, 10);
        [SerializeField] private Vector2 tallBridgeSize = new(10, 25);

        [SerializeField] private Transform iconsParent;

        private readonly Dictionary<Vector2Int, MinimapIcon> _icons = new();
        private Grid _grid;
        
        public void GenerateMinimap(Grid grid)
        {
            _grid = grid;
            _icons.Clear();
            for (int i = iconsParent.childCount-1; i >= 0; i--)
            {
                Destroy(iconsParent.GetChild(i).gameObject);
            }

            HashSet<(Vector2Int, RoomType)> edges = new();
            
            foreach ((Vector2Int index, RoomType type) in grid)
            {
                Vector2Int actualIndex = index - grid.GenerationStart;
                GameObject instantiated = Instantiate(minimapIconPrefab, iconsParent);
                instantiated.transform.localPosition = (Vector2)actualIndex * iconOffsets;
                MinimapIcon icon = instantiated.GetComponent<MinimapIcon>();
                if (grid.FinalRoom == index)
                {
                    icon.RoomIcon = grid.EndingHasBoss ? MinimapIcon.DisplayedIcon.Boss : MinimapIcon.DisplayedIcon.NewRoom;
                } else if (grid.WeaponRooms.Contains(index))
                {
                    icon.RoomIcon = MinimapIcon.DisplayedIcon.Weapon;
                } else if (grid.SmithingRooms.Contains(index))
                {
                    icon.RoomIcon = MinimapIcon.DisplayedIcon.Upgrade;
                }
                _icons.Add(index, icon);
                List<RoomType> currEdges = type.GetParts();
                foreach (RoomType currEdge in currEdges)
                {
                    if (edges.Contains((index, currEdge)) ||
                        edges.Contains((currEdge.Move(index), currEdge.GetOpposing())))
                    {
                        continue;
                    }

                    Vector2 initialPos = actualIndex * iconOffsets;
                    Vector2 bridgeSize;
                    switch (currEdge)
                    {
                        case RoomType.LeftOpen:
                            initialPos -= new Vector2(iconOffsets.x / 2f, 0);
                            bridgeSize = wideBridgeSize;
                            break;
                        case RoomType.TopOpen:
                            initialPos += new Vector2(0, iconOffsets.y / 2f);
                            bridgeSize = tallBridgeSize;
                            break;
                        case RoomType.RightOpen:
                            initialPos += new Vector2(iconOffsets.x / 2f, 0);
                            bridgeSize = wideBridgeSize;
                            break;
                        case RoomType.BottomOpen:
                            initialPos -= new Vector2(0, iconOffsets.y / 2f);
                            bridgeSize = tallBridgeSize;
                            break;
                        default:
                            bridgeSize = Vector2.zero;
                            break;
                    }
                    
                    GameObject bridge = Instantiate(minimapBridgeIconPrefab, iconsParent);
                    RectTransform rect = bridge.GetComponent<RectTransform>();
                    rect.anchoredPosition = initialPos;
                    rect.sizeDelta = bridgeSize;
                    edges.Add((index, currEdge));
                }
            }
        }

        public void UpdateDisplay()
        {
            // TODO ONLY UPDATE A FEW
            foreach ((Vector2Int index, RoomData display) in _grid.GetAllData())
            {
                MinimapIcon.DisplayState newState = MinimapIcon.DisplayState.Unvisited;
                if (display.PlayerPresent)
                {
                    newState = MinimapIcon.DisplayState.Present;
                    iconsParent.transform.localPosition = -(Vector2)(index-_grid.GenerationStart) * iconOffsets;
                }
                else if (display.PlayerVisited) newState = MinimapIcon.DisplayState.Visited;
                _icons[index].State = newState;
            }
        }

        private void FixedUpdate()
        {
            // TODO OPTIMIZE ONLY TRIGGER ON ENTEr
            UpdateDisplay();
        }
    }
}