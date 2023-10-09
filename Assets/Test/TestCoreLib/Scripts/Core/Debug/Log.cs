namespace TestCoreLib
{


    public static class Log
    {
        private static ILog _log;

        public static void Initialize(ILog log)
        {
            _log = log;
        }
        public static void Info(string str)
        {
            _log?.Info(str);
        }
        public static void Warning(string str)
        {
            _log?.Warning(str);
        }
        public static void Error(string str)
        {
            _log?.Error(str);
        }
    }

}