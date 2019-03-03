using System;
using System.IO;

class Room {
  public int width = 0;
  public int height = 0;
  public int x = 0, y = 0;
  public bool is_horizontal = false;
  public int xr = 0, yr = 0, widthr = 0, heightr = 0;
  public int connected_to = 0;
  public int connections = 1;
  public bool connectable = true;

  public Room(int[] _width, int[] _height) {
    Random rnd = new Random();
    this.width = rnd.Next(_width[0], _width[1]);
    this.height = rnd.Next(_height[0], _height[1]);
  }

  public void set_coordinates(int[] coords) {
    this.x = coords[0];
    this.y = coords[1];
  }

  public void init_room(Random rnd) {
    this.widthr = rnd.Next(Convert.ToInt32(this.width / 1.5), Convert.ToInt32(this.width * (5.0/6.0)));
    this.heightr = rnd.Next(Convert.ToInt32(this.height / 1.5), Convert.ToInt32(this.height * (5.0/6.0)));
    this.xr = rnd.Next(0, this.width - this.widthr);
    this.yr = rnd.Next(0, this.height - this.heightr);
  }
}


class Level {
  public int[][] map;
  int width, height;
  public double kmin = 0.3, kmax = 0.7;
  Room[] rooms;
  int rooms_count = 0;
  private int path_size = 4;

  public Level(int[] _width, int[] _height) {
    Random rnd = new Random();
    this.width = rnd.Next(_width[0], _width[1]);
    this.height = rnd.Next(_height[0], _height[1]);
    this.resize(0);

    this.map = new int[this.height][];
    for (int i = 0; i < this.height; i++) {
      this.map[i] = new int[this.width];
      for (int j = 0; j < this.width; j++) {
        this.map[i][j] = 0;
      }
    }
  }

  public Level() {
    this.width = 0;
    this.height = 0;
    this.resize(0);

    this.map = new int[this.height][];
    for (int i = 0; i < this.height; i++) {
      this.map[i] = new int[this.width];
      for (int j = 0; j < this.width; j++) {
        this.map[i][j] = 0;
      }
    }
  }

  private void resize(int k) {
    this.rooms = new Room[k];
  }

  public void print(string path) {
    File.WriteAllText(path, "");
    using (StreamWriter sw = File.AppendText(path)) {
      for (int i = 0; i < this.height; i++) {
        for (int j = 0; j < this.width; j++) {
          sw.Write(this.map[i][j].ToString());
          sw.Write("\t");
        }
        sw.Write("\n");
      }
    }
  }

  private void add_room(Room room) {
    try {
      this.rooms[this.rooms_count] = room;
    } catch (System.Exception) {
      Room[] arr = new Room[rooms_count + 1];
      for (int i = 0; i < this.rooms.Length; i++) {
        arr[i] = this.rooms[i];
      }
      this.rooms = arr;
    }
    this.rooms_count++;
  }

  private void blit(Room room, Random rnd) {
    int kek = rnd.Next(0, 256);
    for (int i = room.x; i < room.x + room.width; i++) {
      for (int j = room.y; j < room.y + room.height; j++) {
        try {
          this.map[j][i] = kek;
          //this.map[j][i] = 0;
        } catch (System.Exception) {

        }
      }
    }
  }

  public void split(Room room, Random rnd, int j) {
    int orientation = 1;
    
    if (room.is_horizontal) {
      orientation = 0;
    } //0 - горизонтально 1 - вертикально
    
    if (orientation == 0) {
      int split_h = rnd.Next(Convert.ToInt32(room.height * this.kmin), Convert.ToInt32(room.height * this.kmax));

      int[][] size = new int[][] {
        new int[] { room.width, room.width },
        new int[] { room.height - split_h, room.height - split_h },
        new int[] { room.x, room.y + split_h},
      };
      Room down_room = new Room(size[0], size[1]);
      down_room.set_coordinates(size[2]);
      down_room.is_horizontal = false;
      this.rooms[j].is_horizontal = false;
      this.rooms[j].height = split_h;
      this.add_room(down_room);
      this.rooms[j].connected_to = this.rooms_count - 1;
      this.rooms[this.rooms_count - 1].connected_to = j;
    } else {
      int split_w = rnd.Next(Convert.ToInt32(room.width * this.kmin), Convert.ToInt32(room.width * this.kmax));

      int[][] size = new int[][] {
        new int[] { room.width - split_w, room.width - split_w },
        new int[] { room.height, room.height },
        new int[] { room.x + split_w, room.y},
      };
      Room down_room = new Room(size[0], size[1]);
      down_room.set_coordinates(size[2]);
      down_room.is_horizontal = true;
      this.rooms[j].is_horizontal = true;
      this.rooms[j].width = split_w;
      this.add_room(down_room);
      this.rooms[j].connected_to = this.rooms_count - 1;
      this.rooms[this.rooms_count - 1].connected_to = j;
    }
  }

  private void draw_room(Room room, Random rnd) {
    room.init_room(rnd);
    for (int i = room.x + room.xr; i < room.x + room.xr + room.widthr; i++) {
      for (int j = room.y + room.yr; j < room.y + room.yr + room.heightr; j++) {
        this.map[j][i] = 1;
      }
    }
  }

  // Это даже не смотрите
  private void build_ends(Random rnd) {
    for (int i = 0; i < this.rooms_count; i++) {
      if (this.rooms[i].connected_to > i) {
        this.connect(this.rooms[i], this.rooms[this.rooms[i].connected_to], rnd);
      }
    }
  }

  private bool all_connected() {
    int[][] arr = this.map;
    bool ok = true;
    int start_pos_x = 0, start_pos_y = 0;

    for (int i = 0; i < arr.Length; i++) {
      for (int j = 0; j < arr[i].Length; j++) {
        if (arr[i][j] == 2) {
          arr[i][j] = 1;
          start_pos_x = j;
          start_pos_y = i;
        }
      }
    }

    arr = this.ebanaya_recursiya(arr, start_pos_x, start_pos_y);

    for (int i = 0; i < arr.Length; i++) {
      for (int j = 0; j < arr[i].Length; j++) {
        if (!(arr[i][j] == 0 || arr[i][j] == -1)) {
          ok = false;
        }
      }
    }

    return ok;
  }

  private int[][] ebanaya_recursiya(int[][] _arr, int start_pos_x, int start_pos_y) {
    int[][] arr = _arr;
    if (!(arr[start_pos_y][start_pos_x] == 0 || arr[start_pos_y][start_pos_x] == -1)) {
      arr[start_pos_y][start_pos_x] = -1;
      try {
        arr = this.ebanaya_recursiya(arr, start_pos_x + 1, start_pos_y);
      } catch (System.Exception) {}
      try {
        arr = this.ebanaya_recursiya(arr, start_pos_x - 1, start_pos_y);
      } catch (System.Exception) {}
      try {
        arr = this.ebanaya_recursiya(arr, start_pos_x, start_pos_y + 1);
      } catch (System.Exception) {}
      try {
        arr = this.ebanaya_recursiya(arr, start_pos_x, start_pos_y - 1);
      } catch (System.Exception) {}
    }
    return arr;
  }

  private void connect(Room r1, Room r2, Random rnd) {
    if (r1.is_horizontal) { // если комнаты разделены вертикальной линией
      int min_h = r1.yr + r1.y;
      if (min_h < r2.y + r2.yr) {
        min_h = r2.y + r2.yr;
      }
      int max_h = r1.yr + r1.y + r1.heightr;
      if (max_h > r2.y + r2.yr + r2.heightr) {
        max_h = r2.y + r2.yr + r2.heightr;
      }

      int center_line = 0;
      try {
        center_line = rnd.Next(min_h + this.path_size / 2, max_h - this.path_size / 2);
      } catch (System.Exception) {
        center_line = (min_h + max_h) / 2;
      }

      for (int i = center_line - this.path_size / 2; i < center_line + this.path_size / 2; i++) {
        for (int j = r1.x + r1.xr + r1.widthr; j < r2.xr + r2.x; j++) {
          this.map[i][j] = 2;
        }
      }
      
    } else {
      int min_w = r1.xr + r1.x;
      if (min_w < r2.x + r2.xr) {
        min_w = r2.x + r2.xr;
      }
      int max_w = r1.x + r1.xr + r1.widthr;
      if (max_w > r2.x + r2.xr + r2.widthr) {
        max_w = r2.x + r2.xr + r2.widthr;
      }

      int center_line = 0;
      try {
        center_line = rnd.Next(min_w+ this.path_size / 2, max_w - this.path_size / 2);
      } catch (System.Exception) {
        center_line = (min_w + max_w) / 2;
      }

      for (int i = center_line - this.path_size / 2; i < center_line + this.path_size / 2; i++) {
        for (int j = r1.y + r1.yr + r1.heightr; j < r2.y + r2.yr; j++) {
          this.map[j][i] = 2;
        }
      }
    }
  }

  public void generate(int dungeon_count, int min) {
    Random rnd = new Random();
    this.path_size = min;

    int[][] size = new int[][] {
      new int[] {width, width},
      new int[] {height, height},
      new int[] {0, 0},
    };
    Room bg_room = new Room(size[0], size[1]);
    bg_room.set_coordinates(size[2]);
    bg_room.is_horizontal = rnd.Next(0, 2) == 1;
    this.resize(Convert.ToInt32(Math.Pow(2, dungeon_count)));
    this.add_room(bg_room);

    int[] data = new int[10];
    data[0] = 0;
    for (int i = 0; i < dungeon_count; i++) {
      data[i + 1] = Convert.ToInt32(Math.Pow(2, i));
    }

    for (int i = 0; i < dungeon_count; i++) {
      for (int j = 0; j < data[i]; j++) {
        this.split(this.rooms[j], rnd, j);
      }
    }

    for (int i = 0; i < this.rooms_count; i++) {
      // Функция для отрисовки всей комнаты чтобы её потом визуализировать
      //this.blit(this.rooms[i], rnd);
      // Вот в этой функции надо бы делать проверку на соответствие комнаты размерам
      this.draw_room(this.rooms[i], rnd);
    }

    // Вот эта херня в теории должна рисовать проходы между комнатами, но поа не рисует
    this.build_ends(rnd);
    //this.print("data.txt");
  }

  
  public int[][] generate(int width, int height, int deepth, int path_width) {
    // Скрипт пишет поле в файл "data.txt"
    int[][] size = new int[][] {
      new int[] { width, width }, // Пределы для ширины
      new int[] { height, height }, // Пределы для высоты
    };
    Level lvl = new Level(size[0], size[1]);
    lvl.generate(deepth, path_width); // Глубина дерева или я хуй знает как это называется и ширина прохода
    return lvl.map;
  }
}


class MainClass {
  public static void Main (string[] args) {
    Level lvl = new Level();
    //Вот тебе и карта (последние 2 параметра не трогай пожалуйста, они пока только так работают :<)
    int[][] kek = lvl.generate(10, 10, 2, 2);
    
    /*for (int i = 0; i < kek.Length; i++) {
      for (int j = 0; j < kek[0].Length; j++) {
        Console.Write(kek[i][j]);
        Console.Write(" ");
      }
      Console.WriteLine(" ");
    }*/
  }
}
