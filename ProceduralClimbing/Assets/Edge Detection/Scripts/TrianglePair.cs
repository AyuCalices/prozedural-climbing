namespace Edge_Detection.Scripts
{
    public class TrianglePair
    {
        public readonly Triangle t0;
        public Triangle t1;

        private bool _filled;

        public TrianglePair(Triangle t0)
        {
            this.t0 = t0;
            _filled = false;
        }
        
        public void Add(Triangle triangle)
        {
            if (_filled) return;
            t1 = triangle;
            _filled = true;
        }
    }
}
