using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
namespace HelperTools {
    internal class PerformanceCheck {


        /*
         * 
         * 
         * SELECT p.*
         * FROM Pizza p
         * JOIN PT pt ON p.ID = pt.PizzaID
         * JOIN Toppings t ON pt.ToppingID = t.ID
         * WHERE t.name LIKE 't%'
         *  AND (t.name = 'cheese' OR t.name = 'sauce');
         * 
         * Another approach
         * I think one long comparison is better than multiple runs of small comparisons... Disk scanning wise
         * 
         * SELECT p.*
         * FROM Pizza p
         * JOIN PT pt ON p.ID = pt.PizzaID
         * WHERE pt.ToppingID IN (SELECT ID FROM Toppings WHERE name LIKE 't%')
         *      AND pt.ToppingID IN (SELECT ID FROM Toppings WHERE name IN ('cheese', 'sauce'));
         * 
         * 
         * 
         * 
         * 
         * 
         */

        public static void CheckStringWithEachID(SQLiteConnection conn) {
            Stopwatch sw = new Stopwatch();
            int count = 0;

            sw.Start();
            // AND happens before OR
            using (var cmd = new SQLiteCommand(conn)) {
                cmd.CommandText = @"
                    SELECT act.first_name,act.last_name, fi.title  FROM actor act
                    JOIN film_actor fa ON act.actor_id = fa.actor_id
                    JOIN film fi ON fa.film_id = fi.film_id
                    WHERE LOWER(fi.title) LIKE LOWER('t%')
                        AND LOWER(fi.title) LIKE LOWER('%r')
                        OR LOWER(fi.title) LIKE LOWER('%bird%')
                        OR LOWER(fi.title) LIKE LOWER('%house%')
                    ";
                var reader = cmd.ExecuteReader();
                while (reader.Read()) {
                    //Console.WriteLine(string.Format("{0,-12} - {1,-13} {2,-20}", reader.GetValue(0),reader.GetValue(1),reader.GetValue(2)));
                    count++;
                }
            }
            sw.Stop();
            Console.WriteLine("Result query count = " + count);
            Console.WriteLine("Elapsed={0}", sw.Elapsed); //00:00:00.0539724
        }
        public static void CheckIfInIDs(SQLiteConnection conn) {
            Stopwatch sw = new Stopwatch();
            int count = 0;
            sw.Start();
            using (var cmd = new SQLiteCommand(conn)) {
                cmd.CommandText = @"
                    SELECT act.* FROM actor act
                    JOIN film_actor fa ON act.actor_id = fa.actor_id
                    JOIN film fi ON fa.film_id = fi.film_id
                    WHERE LOWER(fi.film_id)  IN (SELECT film_id FROM film WHERE LOWER(title) LIKE LOWER('t%'))
                        AND LOWER(fi.film_id)  IN (SELECT film_id FROM film WHERE LOWER(title) LIKE LOWER('%r'))
                        OR LOWER(fi.film_id)  IN (SELECT film_id FROM film WHERE LOWER(title) LIKE LOWER('%bird%'))
                        OR LOWER(fi.film_id)  IN (SELECT film_id FROM film WHERE LOWER(title) LIKE LOWER('%house%'))
                    ";
                var reader = cmd.ExecuteReader();
                while (reader.Read()) {
                    //Console.WriteLine(string.Format("{0,-12} - {1,-13} {2,-20}", reader.GetValue(0), reader.GetValue(1), reader.GetValue(2)));
                    count++;
                }
            }
            sw.Stop();
            Console.WriteLine("Result query count = " + count);
            Console.WriteLine("Elapsed={0}", sw.Elapsed);
        }
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
        /*
        Results over multiple runs
        for just first like
       Result query count = 243
        Elapsed=00:00:00.0123900
        Result query count = 243
        Elapsed=00:00:00.0020370
        ratio 6.082

        Result query count = 58
        Elapsed=00:00:00.0146885
        Result query count = 58
        Elapsed=00:00:00.0040588
        ratio 3.6189

        Result query count = 31
        Elapsed=00:00:00.0058455
        Result query count = 31
        Elapsed=00:00:00.0059143

        Result query count = 87
        Elapsed=00:00:00.0080970
        Result query count = 87
        Elapsed=00:00:00.0065998


        Result query count = 77
        Elapsed=00:00:00.0049120
        Result query count = 77
        Elapsed=00:00:00.0047175

         */
        static void Main(string[] args) {
            IntPtr ptr = GetConsoleWindow();
            MoveWindow(ptr, 1200, 1300, 1000, 500, true);

            using (var conn = new SQLiteConnection("Data Source=" + "sqlite-sakila.db")){
                conn.Open();
                CheckStringWithEachID(conn); // connection first time overhead? slower for some reason
                CheckStringWithEachID(conn);
                CheckIfInIDs(conn);
                conn.Close();

            }

        }
    }
}
