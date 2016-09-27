using System.IO;

namespace tankmap {
    public class Map {

        public const int BLOCK_PASS = 0;
        public const int BLOCK_TREE = 1;
        public const int BLOCK_BRICK = 2;
        public const int BLOCK_STEEL = 3;
        public const int BLOCK_WATER = 4;
        public const int BLOCK_EAGLE = 5;
        public const int BLOCK_HERO = 6;

        private int width;
        private int height;
        private int heroX;
        private int heroY;
        private int[] blockData;

        /// <summary>
        /// Constructs a map.
        /// </summary>
        public Map(int w, int h) {
            width = w;
            height = h;
            heroX = -1;
            heroY = -1;
            blockData = new int[width * height];
            for (var i = 0; i < width * height; i++)
                Set(i, BLOCK_PASS);
        }

        /// <summary>
        /// Opens a map file.
        /// </summary>
        /// <param name="fileName">Filename to open.</param>
        public void Open(string fileName) {
            var stream = new FileStream(fileName, FileMode.Open);
            var reader = new BinaryReader(stream);
            try {
                // reads header, 8 bytes
                // header=TANK####, # = ASCII 0
                byte[] check = { 0x54, 0x41, 0x4E, 0x4B, 0x00, 0x00, 0x00, 0x00, };
                var header = reader.ReadBytes(8);
                for (int i = 0; i < header.Length; i++)
                    if (header[i] != check[i])
                        throw new IOException("Header is not acceptable.");

                // reads header information
                // 4 int = width, height, hero x, hero y
                int w = reader.ReadInt32();
                int h = reader.ReadInt32();
                int x = reader.ReadInt32();
                int y = reader.ReadInt32();

                // reads block data
                int size = w * h;
                int[] blockData = new int[size];
                for (int i = 0; i < size; i++)
                    blockData[i] = reader.ReadInt32();

                // copying data
                this.width = w;
                this.height = h;
                this.heroX = x;
                this.heroY = y;
                this.blockData = blockData;
            }
            finally {
                reader.Close();
                stream.Close();
            }
        }

        /// <summary>
        /// Saves a map file.
        /// </summary>
        /// <param name="fileName">Filename to save.</param>
        public void Save(string fileName) {
            var stream = new FileStream(fileName, FileMode.Create);
            var writer = new BinaryWriter(stream);
            try {
                // writes header, 8 bytes
                // header=TANK####, # = ASCII 0
                byte[] check = { 0x54, 0x41, 0x4E, 0x4B, 0x00, 0x00, 0x00, 0x00, };
                writer.Write(check);

                // writes header information
                // 4 int = width, height, hero x, hero y
                writer.Write(width);
                writer.Write(height);
                writer.Write(heroX);
                writer.Write(heroY);

                // writes block data
                int size = width * height;
                for (int i = 0; i < size; i++)
                    writer.Write(blockData[i]);
            }
            finally {
                writer.Close();
                stream.Close();
            }
        }

        public int GetWidth() {
            return width;
        }

        public int GetHeight() {
            return height;
        }

        public int GetHeroX() {
            return heroX;
        }

        public int GetHeroY() {
            return heroY;
        }

        public void SetHeroXY(int x, int y) {
            heroX = x;
            heroY = y;
        }

        public int Get(int index) {
            return blockData[index];
        }

        public void Set(int index, int data) {
            blockData[index] = data;
        }

    }
}
