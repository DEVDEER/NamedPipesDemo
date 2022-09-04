namespace Logic
{
    public class ReadFileToStream
    {
        #region member vars

        private readonly string fn;
        private readonly StreamString ss;

        #endregion

        #region constructors and destructors

        public ReadFileToStream(StreamString str, string filename)
        {
            fn = filename;
            ss = str;
        }

        #endregion

        #region methods

        public void Start()
        {
            var contents = File.ReadAllText(fn);
            ss.WriteString(contents);
        }

        #endregion
    }
}