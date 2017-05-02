using UnityEngine;


    class Wall
    {
        public GameObject plane;
        public Vector2 a;
        public Vector2 b;
        public int orientation;
        public Wall aNeighbour;
        public Wall bNeighbour;

        public Wall(GameObject plane, Vector2 a, Vector2 b, int orientation)
        {
            this.plane = plane;
            this.a = a;
            this.b = b;
            this.orientation = orientation;
            aNeighbour = null;
            bNeighbour = null;
        }
    }