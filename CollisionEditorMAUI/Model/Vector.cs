namespace CollisionEditor.Model
{
    public struct Vector2<T>
    {
        public T X { get; set; }
        public T Y { get; set; }

        public Vector2(T x, T y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(Vector2<T> obj)
        {
            bool conditionX = X is null || obj.X is null ? 
                X is null && obj.X is null : X.Equals(obj.X);

            bool conditionY = Y is null || obj.Y is null ? 
                Y is null && obj.Y is null : Y.Equals(obj.Y);

            return conditionX && conditionY;
        }
    }
}
