﻿using System;
//using ProberInterfaces.ThreadSync;

namespace EasyModbus
{
    /// <summary>
    /// Store Log-Data in a File
    /// </summary>
    public sealed class StoreLogData
    {
        private String filename = null;
        private static volatile StoreLogData instance;
        //private static LockKey syncObject = new LockKey("Store log data");
        private static object syncObject = new object();

        /// <summary>
        /// Private constructor; Ensures the access of the class only via "instance"
        /// </summary>
        private StoreLogData()
        {
        }

        /// <summary>
        /// Returns the instance of the class (singleton)
        /// </summary>
        /// <returns>instance (Singleton)</returns>
        public static StoreLogData Instance
        {
            get
            {
                if (instance == null)
                {
                    //using (Locker locker = new Locker(syncObject))
                    //{
                    lock (syncObject)
                    {
                        if (instance == null)
                            instance = new StoreLogData();
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// Store message in Log-File
        /// </summary>
        /// <param name="message">Message to append to the Log-File</param>
        public void Store(String message)
        {
            if (this.filename == null)
                return;

            using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(Filename, true))
            {
                file.WriteLine(message);
            }
        }

        /// <summary>
        /// Store message in Log-File including Timestamp
        /// </summary>
        /// <param name="message">Message to append to the Log-File</param>
        /// <param name="timestamp">Timestamp to add to the same Row</param>
        public void Store(String message, DateTime timestamp)
        {
            try
            {
                using (System.IO.StreamWriter file =
                    new System.IO.StreamWriter(Filename, true))
                {
                    file.WriteLine(timestamp.ToString("dd.MM.yyyy H:mm:ss.ff ") + message);
                }
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Gets or Sets the Filename to Store Strings in a File
        /// </summary>
        public string Filename
        {
            get
            {
                return filename;
            }
            set
            {
                filename = value;
            }
        }
    }
}
