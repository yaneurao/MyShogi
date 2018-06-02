namespace MyShogi.Model.Math
{
    /// <summary>
    /// 2次元ベクトル
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct Vector2D<T>
    {
        public T X;
        public T Y;

        /// <summary>
        /// (X,Y)に値を設定する。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void SetValue(T x , T y)
        {
            X = x;
            Y = y;
        }
    }


}
