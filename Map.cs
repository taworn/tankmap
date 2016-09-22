using System.IO;

namespace tankmap {
    public class Map {

        private int width;
        private int height;
        private byte[] mapData;
        private int[] imageData;
        private int[] blockData;

        /// <summary>
        /// Constructs a map.
        /// </summary>
        public Map(int w, int h) {
            width = w;
            height = h;
            mapData = new byte[width * height];
            imageData = new int[width * height];
            blockData = new int[width * height];

            for (var i = 0; i < width * height; i++)
                Set(i, 0);
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
                // header=CAPMAP##, # = ASCII 0
                byte[] check = { 0x43, 0x41, 0x50, 0x4D, 0x41, 0x50, 0x00, 0x00, };
                var header = reader.ReadBytes(8);
                for (int i = 0; i < header.Length; i++)
                    if (header[i] != check[i])
                        throw new IOException("Header is not acceptable.");

                // reads header information
                // 6 int = width, height, divo start width, divo start height, pacman start width, pacman start height
                int w = reader.ReadInt32();
                int h = reader.ReadInt32();
                int size = w * h;

                // mapData and imageData are for game, not used map editor, just copying
                byte[] mapData = new byte[size];
                for (int i = 0; i < size; i++)
                    mapData[i] = reader.ReadByte();
                int[] imageData = new int[size];
                for (int i = 0; i < size; i++)
                    imageData[i] = reader.ReadInt32();

                // reads block data
                int[] blockData = new int[size];
                for (int i = 0; i < size; i++)
                    blockData[i] = reader.ReadInt32();

                // copying data
                this.width = w;
                this.height = h;
                this.mapData = mapData;
                this.imageData = imageData;
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
                // header=CAPMAP##, # = ASCII 0
                byte[] check = { 0x43, 0x41, 0x50, 0x4D, 0x41, 0x50, 0x00, 0x00, };
                writer.Write(check);

                // writes header information
                // 6 int = width, height, divo start width, divo start height, pacman start width, pacman start height
                writer.Write(width);
                writer.Write(height);
                int size = width * height;

                // writes map data and image data for game
                for (int i = 0; i < size; i++)
                    writer.Write(mapData[i]);
                for (int i = 0; i < size; i++)
                    writer.Write(imageData[i]);

                // writes block data
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

        public int Get(int index) {
            return blockData[index];
        }

        public void Set(int index, int data) {
            blockData[index] = data;

            // translate block to map and image
            if (data == 0) {
                // blocking
                mapData[index] = 0x01;
                imageData[index] = 3;
            }
            else if (data == 1) {
                // movable
                mapData[index] = 0x00;
                imageData[index] = 0;
            }
            else {
                // treat like blocking
                mapData[index] = 0x01;
                imageData[index] = 3;
            }
        }

    }
}
