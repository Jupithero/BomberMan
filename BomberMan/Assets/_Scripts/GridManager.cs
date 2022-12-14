using System.Collections.Generic;
using OknaaEXTENSIONS;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BomberMan {
    public class GridManager : MonoBehaviour {
        [Header("Grid Dimensions")] public int height = 10;
        public int width = 15;

        [Header("Indestructible Blocks")] public int indestructibleCount = 30;
        [Range(0.0f, 1.0f)] public float indestructibleSpawnChance = 0.4f;

        [Header("Destructible Blocks")] public int destructibleCount = 30;
        [Range(0.0f, 1.0f)] public float destructibleSpawnChance = 0.4f;
        public int playerDestructibleSafeZoneDiameter = 2;
        
        

        [Header("Enemies")] public int enemyCount = 5;
        [Range(0.0f, 1.0f)] public float enemySpawnChance = 0.4f;
        public int playerEnemySafeZoneDiameter = 2;

        [HideInInspector] public Player Player;

        private Camera _mainCamera;
        private Tile[,] _gridTiles;
        private int _indestructiblesInstantiated = 0;
        private int _destructiblesInstantiated = 0;
        private int _enemiesInstantiated = 0;
        private float cameraOffset = 0.5f;
        private List<Tile> _tileTypes;
        private List<Unit> _enemies;

        private void Start() {
            Enemy.EnemyCount = enemyCount;
            _mainCamera = Camera.main;
            _gridTiles = new Tile[width, height];

            while (GameObject.FindGameObjectsWithTag("Enemy").Length < enemyCount) {
                GenerateGrid();
            }
        }

        #region GridGeneration

        private void GenerateGrid() {
            _tileTypes = Resources.LoadAll<Tile>("Tiles").ToList();
            _enemies = Resources.LoadAll<Unit>(PathVariables.EnemiesFolder).ToList();

            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    GenerateTiles(y, x);
                    GenerateBorders(y, x);
                    GeneratePillars(y, x);
                    GenerateDestructibleBlocks(y, x);
                    GenerateEnemies(y, x);
                }
            }

            GeneratePlayer();

            _mainCamera.transform.position = new Vector3(width * 0.5f - 0.5f, height * 0.5f - 0.5f + cameraOffset, -10);
        }
        
        private void GenerateTiles(int y, int x) {
            var randomTile = GetRandomTileByWeight(_tileTypes);
            GameObject tileInstance = Instantiate(randomTile, new Vector3(x, y), Quaternion.identity, transform);
            Tile tile = tileInstance.GetComponent<Tile>();
            tile.Init(this, randomTile.name, x, y);
        }

        private void GenerateBorders(int y, int x) {
            bool isBorder = y == 0 || y == height - 1 || x == 0 || x == width - 1;
            if (!isBorder) return;
            SetupUnite(Resources.Load<Tile>(PathVariables.Border).gameObject, x, y);
        }

        private void GeneratePillars(int y, int x) {
            if (_indestructiblesInstantiated >= indestructibleCount) return; 
            if (IsPlayerSaveZone(playerDestructibleSafeZoneDiameter, y, x)) return; 
            if (_gridTiles[x, y] != null && !_gridTiles[x, y].IsFree()) return; 
            if (Random.Range(0f, 1f) > indestructibleSpawnChance) return; 

            SetupUnite(Resources.Load<Tile>(PathVariables.Pillar).gameObject, x, y);
            _indestructiblesInstantiated++;
            

            void GenerateAtPredeterminedLocations() {
                bool isBorder = y == 0 || y == height - 1 || x == 0 || x == width - 1;
                if (isBorder) return;
                if (y % 2 == 1 || x % 2 == 1) return;
                SetupUnite(Resources.Load<Tile>(PathVariables.Pillar).gameObject, x, y);
            }
        }

        private void GenerateDestructibleBlocks(int y, int x) {
            if (_destructiblesInstantiated >= destructibleCount) return; // if all destructible blocks are instantiated, do not generate any more
            if (IsPlayerSaveZone(playerDestructibleSafeZoneDiameter, y, x))
                return; // To leave some space for the player to dodge the first bomb, no obstacles are allowed in his spawn point
            if (_gridTiles[x, y] != null && !_gridTiles[x, y].IsFree()) return; // if tile is not free, do not generate destructible block
            if (Random.Range(0f, 1f) > destructibleSpawnChance) return; // To generate destructible blocks 50% of the time

            SetupUnite(Resources.Load<Tile>(PathVariables.DestructibleBlock).gameObject, x, y);
            _destructiblesInstantiated++;
        }

        private void GenerateEnemies(int y, int x) {
            if (_enemiesInstantiated >= enemyCount) return;
            if (IsPlayerSaveZone(playerEnemySafeZoneDiameter, y, x)) return;
            if (_gridTiles[x, y] != null && !_gridTiles[x, y].IsFree()) return;
            if (Random.Range(0f, 1f) > enemySpawnChance) return;

            SetupUnite(_enemies[_enemiesInstantiated].gameObject, x, y);
            _enemiesInstantiated++;
        }

        private void GeneratePlayer() {
            Player = SetupUnite(Resources.Load<Tile>(PathVariables.Player).gameObject, 1, height - 2).GetComponent<Player>();
        }

        private GameObject SetupUnite(GameObject tileGameObject, int x, int y) {
            GameObject unit = Instantiate(tileGameObject, new Vector3(x, y, 0), Quaternion.identity, transform);
            Tile tile = unit.GetComponent<Tile>();
            tile.Init(this, tileGameObject.name, x, y);
            tile.SetFree(false);
            _gridTiles[x, y] = tile;
            return unit;
        }

        private bool IsPlayerSaveZone(int saveZoneDiameter, int y, int x) {
            int playerSpawnX = 1;
            int playerSpawnY = height - 2;

            int playerSafeZoneMaxX = playerSpawnX + saveZoneDiameter;
            int playerSafeZoneMaxY = playerSpawnY - saveZoneDiameter;

            return x >= playerSpawnX && x <= playerSafeZoneMaxX && y <= playerSpawnY && y >= playerSafeZoneMaxY;
        }
        
        private GameObject GetRandomTileByWeight(List<Tile> tiles) {
            float totalWeight = 0;
            foreach (var tile in tiles) totalWeight += tile.GetWeight();

            float random = Random.Range(0f, totalWeight);

            foreach (var tile in tiles) {
                random -= tile.GetWeight();
                if (random <= 0) return tile.gameObject;
            }

            return tiles[0].gameObject;
        }

        #endregion

     
        public Vector2Int GetCoordFromPosition(Vector3 position) => new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
        public Tile GetTileFromCoord(int x, int y) => _gridTiles[x, y];
        
        public List<Tile> GetSurroundingTiles(Tile tile) {
            var tilesList = new List<Tile>();
            var x = tile.GetX();
            var y = tile.GetY();

            if (x > 0) tilesList.Add(_gridTiles[x - 1, y]);
            if (x < width - 1) tilesList.Add(_gridTiles[x + 1, y]);
            if (y > 0) tilesList.Add(_gridTiles[x, y - 1]);
            if (y < height - 1) tilesList.Add(_gridTiles[x, y + 1]);

            return tilesList;
        }

        public void PlaceBomb(Vector3 transformPosition, int bombRange, int bombTimer) {
            var bombGameObject = Resources.Load<Tile>(PathVariables.Bomb).gameObject;
            var positionX = Mathf.RoundToInt(transformPosition.x);
            var positionY = Mathf.RoundToInt(transformPosition.y);
            var position = new Vector3(positionX, positionY, 0);
            
            var unitGameObject = Instantiate(bombGameObject, position, Quaternion.identity, transform);
           
            var bombScript = unitGameObject.GetComponent<Bomb>();
            bombScript.Init(this, bombGameObject.name, (int)position.x, (int)position.y, bombTimer, bombRange);
        }

        public static void RestartScene() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}