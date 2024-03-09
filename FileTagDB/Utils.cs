namespace FileTagDB {
    public static class Utils {

        static Action<string>? printer;
        public static readonly List<string> Empty= new List<string>();
        public static void SetPrinter(Action<string> outputter) {
            printer = outputter;
        }
        public static void LogToOutput(string msg) {
            if (printer == null) {
                Console.WriteLine(msg);
                return;
            }
            try {
                printer(msg);
            }catch(Exception e) {
                Console.WriteLine(msg);
                printer = null;
            }
        }
        public static int Count(string str, char c) {
            int count = 0;
            for(int i = 0; i < str.Length; i++) {
                if (str[i] == c)
                    count++;
            }
            return count;
        }
    }
}
