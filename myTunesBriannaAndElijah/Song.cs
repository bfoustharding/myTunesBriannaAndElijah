namespace WpfMusicRepoDemo
{
    public class Song
    {
        public int Id { set; get; }
        public string Title { set; get; } = "";
        public string Artist { set; get; } = "";
        public string Album { set; get; } = "";
        public string Genre { set; get; } = "";
        public string Length { set; get; } = "";
        public string Filename { set; get; } = "";
        public string AlbumImageUrl { set; get; } = "";
        public string AboutUrl { set; get; } = "";


        // Two songs are equal if all their properties are equal
        public override bool Equals(object? obj)
        {
            if (obj is not Song s)
            {
                return false;
            }

            return s.Id == Id && s.Title == Title && s.Artist == Artist &&
                s.Album == Album && s.Genre == Genre && s.Length == Length &&
                s.Filename == Filename;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
