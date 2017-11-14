using System;

namespace DefaultNamespace
{
    public class SeriesInfo: IEquatable<SeriesInfo>
    {
        public int Series;
        public int Score;

        public SeriesInfo(int series, int score)
        {
            Series = series;
            Score = score;
        }

        public SeriesInfo()
        {
        }

        public bool Equals(SeriesInfo obj)
        {
            if (obj == null)
                return false;
            return Series == obj.Series;
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            SeriesInfo personObj = obj as SeriesInfo;
            return personObj != null && Equals(personObj);
        }
    }
}