using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;

namespace MediaPreview
{
    class Files : IDisposable
    {
        private MFile[] mfiles = null;
        private object lockObj = new object();
        private int rows = 3;
        private int cols = 3;
        private string[] allSingleFiles = null;
        private bool allSingleFilesInited = false;
        private int allSingleFilesIndex = 0;
        private string startPath = null;
        private bool startPathIsDir = false;
        public volatile bool HasData = false;

        private double _allSingleFilesIndexPercent = -1.0;
        private object _allSingleFilesIndexPercentLock = new object();

        public delegate bool DShouldStop();
        public DShouldStop stop = null;
        
        public bool ShouldStop()
        {
            if (stop == null) return false;
            return stop();
        }

        private void UpdateDisplayProgressPercent(int index) 
        {
            double percent = -1.0;
            if (allSingleFiles != null)
            {
                percent = 0.0;
                if ((allSingleFiles.Length == 0) || (index >= allSingleFiles.Length))
                {
                    percent = 100.0;
                }
                else
                {
                    if (index <= 0)
                    {
                        percent = 0.0;
                    }
                    else
                    {
                        percent = index * 100 / allSingleFiles.Length;
                    }
                }
            }
            DisplayProgressPercent = percent;
        }

        public double DisplayProgressPercent 
        {
            get 
            {
                lock (_allSingleFilesIndexPercentLock)
                {
                    return _allSingleFilesIndexPercent;
                }
            }
            set
            {
                lock (_allSingleFilesIndexPercentLock) 
                {
                    _allSingleFilesIndexPercent = value;
                }
            }
        }

        public void Dispose() 
        {
            lock (this)
            {
                ClearMFiles();
                mfiles = null;
                allSingleFiles = null;
            }
        }

        public bool SetStartPath(string startPath)
        {
            lock (this.lockObj)
            {
                _allSingleFilesIndexPercent = -1.0;
                HasData = false;
                allSingleFilesInited = false;
                this.startPath = null;
                this.startPathIsDir = false;
                allSingleFiles = null;
                allSingleFilesIndex = 0;
                ClearMFiles();

                if (string.IsNullOrEmpty(startPath))
                {
                    return false;
                }
                if (string.IsNullOrEmpty(startPath)) return false;
                bool isDir = Directory.Exists(startPath);
                if (!(File.Exists(startPath) || isDir))
                {
                    return false;
                }
                rows = cols = 0;
                this.startPath = startPath;
                this.startPathIsDir = isDir;
                return true;
            }
        }

        public bool StartPathIsDir 
        {
            get
            {
                lock (this)
                {
                    return this.startPathIsDir;
                }
            }
        }

        public volatile bool isMoviePreview = false;
        public int Apply(bool isSlideShow)
        {
            isMoviePreview = false;
            lock (this)
            {
                if (string.IsNullOrEmpty(this.startPath))
                {
                    return 0;
                }
                if (this.startPathIsDir)
                {
                    InitDirFiles();
                    FillNextPage();
                }
                else
                {
                    FillOnePage(isSlideShow);
                }
                return Count;
            }
        }

        private void InitMFiles()
        {
            lock (lockObj)
            {
                ClearMFiles();
                int m = rows * cols;
                if (m == 0) return;
                if ((mfiles == null)
                    || (mfiles.Length != m))
                {
                    mfiles = new MFile[m];
                    for (int i = 0; i < m; i++)
                    {
                        mfiles[i] = new MFile();
                    }
                    HasData = true;
                }
            }
        }

        private void ClearMFiles()
        {
            lock (lockObj)
            {
                if (mfiles == null) return;
                for (int i = 0; i < mfiles.Length; i++)
                {
                    mfiles[i].Dispose();
                }
                mfiles = null;
                HasData = false;
            }
        }

        private void FillNextPage()
        {
            lock (lockObj)
            {
                if (allSingleFiles == null) 
                {
                    return;
                }
                this.InitMFiles();
                if (allSingleFilesIndex < 0)
                {
                    allSingleFilesIndex = 0;
                }
                int m = rows * cols;
                for (int i = 0; i < m; i++)
                {
                    if (ShouldStop()) return;
                    MFile mf = GetAt(i);
                    if (mf == null) 
                    {
                        break;
                    }
                    int fidx = allSingleFilesIndex + i;
                    if (fidx >= allSingleFiles.Length) break;
                    mf.Update(allSingleFiles[fidx], null, null, MFile.ImageType.None);
                }
                UpdateDisplayProgressPercent(this.allSingleFilesIndex + m);
            }
        }

        private void FillOnePage(bool isSlideShow)
        {
            this.isMoviePreview = false;
            lock (lockObj)
            {
                if (string.IsNullOrEmpty(this.startPath))
                {
                    return;
                }
                MFile.ImageType imgType = MFile.ImageType.Movie;
                TimeSpan tsMax = TimeSpan.MaxValue;
                if (Magic.IsImage(this.startPath))
                {
                    imgType = MFile.ImageType.Image;
                }
                else
                {
                    if (isSlideShow)
                    {
                        imgType = MFile.ImageType.MovieImage;
                    }
                }
                if (imgType == MFile.ImageType.Movie)
                {
                    tsMax = Imager.GetLength(this.startPath);
                }
                if ((imgType != MFile.ImageType.Movie) || (tsMax == TimeSpan.MaxValue))
                {
                    rows = 1;
                    cols = 1;
                }
                else
                {
                    this.isMoviePreview = true;
                    rows = 3;
                    cols = 3;
                }
                InitMFiles();
                int m = rows * cols;
                string previousTime = string.Empty;
                for (int i = 0; i < m; i++)
                {
                    if (ShouldStop()) return;
                    MFile mf = GetAt(i);
                    if (mf == null) 
                    {
                        break;
                    }
                    
                    string time = string.Empty;
                    string timeShort = string.Empty;
                    if ((imgType == MFile.ImageType.Movie))
                    {
                        time = Imager.GetTime(i, m, tsMax, false);
                        timeShort = Imager.GetTime(i, m, tsMax, true);
                        if ((i > 0) && (time == previousTime))
                        {
                            break;
                        }
                        previousTime = time;
                    }
                    mf.Update(this.startPath, time, timeShort, imgType);
                }
                UpdateDisplayProgressPercent(this.allSingleFilesIndex);
            }
        }

        private void InitDirFiles()
        {
            lock (lockObj)
            {
                if (!allSingleFilesInited)
                {
                    allSingleFilesInited = true;
                    this.allSingleFilesIndex = 0;
                    if (string.IsNullOrEmpty(this.startPath)) return;
                    this.allSingleFiles = GetFiles(this.startPath);
                    if ((this.allSingleFiles == null) || (this.allSingleFiles.Length <= 0)) 
                    {
                        this.allSingleFiles = null;
                        return;
                    }

                    rows = (int)Math.Sqrt(allSingleFiles.Length);
                    cols = rows;
                    if ((rows * cols) < allSingleFiles.Length)
                    {
                        rows++;
                    }
                    if ((rows * cols) < allSingleFiles.Length)
                    {
                        cols++;
                    }
                    int maxRows = Program.maxRows;
                    if (rows < 1) rows = 1;
                    if (rows > maxRows) rows = maxRows;
                    if (cols < 1) cols = 1;
                    if (cols > maxRows) cols = maxRows;
                    InitMFiles();
                }
            }
        }

        #region dirfiles

        private string[] GetFiles(string dir)
        {
            List<string> f = new List<string>();
            string[] files = null;
            try
            {
                files = Directory.GetDirectories(dir);
            }
            catch { }
            if ((files != null) && (files.Length > 0))
            {
                Array.Sort(files, ns.StringLogicalComparer.Default);
                for (int i = 0; i < files.Length; i++)
                {
                    if (this.ShouldStop()) return null;
                    if (IsSystemHidden(files[i])) continue;
                    f.Add(files[i]);
                }
            }
            try
            {
                files = Directory.GetFiles(dir);
            }
            catch { }
            if ((files != null) && (files.Length > 0))
            {
                Array.Sort(files, ns.StringLogicalComparer.Default);
                for (int i = 0; i < files.Length; i++)
                {
                    if (this.ShouldStop()) return null;
                    if (IsSystemHidden(files[i])) continue;
                    f.Add(files[i]);
                }
            }
            if (f.Count <= 0) return null;
            files = f.ToArray();
            return files;
        }

        public static bool IsSystemHidden(string file)
        {
            return IsSystemHidden(File.GetAttributes(file));
        }

        private static bool IsSystemHidden(FileAttributes fa)
        {
            if ((fa & FileAttributes.Hidden) == FileAttributes.Hidden)
            {
                return true;
            }
            if ((fa & FileAttributes.System) == FileAttributes.System)
            {
                return true;
            }
            return false;
        }

        #endregion dirfiles
              

        #region move

        public bool canLoop = false;
        private volatile bool withinMoveNext = false;
        public enum MoveDirection { Down, Up, Head, End, PageDown, PageUp }
        public bool MoveToNextFile(MoveDirection direction)
        {
            if (withinMoveNext) return false;
            lock (this.lockObj)
            {
                try
                {
                    if (string.IsNullOrEmpty(this.startPath)) return false;
                    withinMoveNext = true;
                    int newIndex = -1;
                    if (this.startPathIsDir)
                    {
                        if (allSingleFiles == null)
                        {
                            return false;
                        }
                        newIndex = this.allSingleFilesIndex;
                        int pageSize = rows * cols;
                        switch (direction)
                        {
                            case MoveDirection.PageDown:
                            case MoveDirection.Down:
                                newIndex += pageSize;
                                break;
                            case MoveDirection.PageUp:
                            case MoveDirection.Up:
                                newIndex -= pageSize;
                                break;
                            case MoveDirection.Head:
                                newIndex = 0;
                                break;
                            case MoveDirection.End:
                                newIndex = -1;
                                break;
                        }
                        if (newIndex >= allSingleFiles.Length)
                        {
                            if (!canLoop) 
                            {
                                return false;
                            }
                            newIndex = 0;
                        }
                        if (newIndex < 0)
                        {
                            if ((direction != MoveDirection.End) && !canLoop)
                            {
                                return false;
                            }
                            newIndex = (this.allSingleFiles.Length / pageSize) - 1;
                            if (this.allSingleFiles.Length % pageSize > 0) newIndex++;
                            newIndex = newIndex * pageSize;
                        }
                        if (newIndex == this.allSingleFilesIndex)
                        {
                            return false;
                        }
                        this.allSingleFilesIndex = newIndex;
                        return true;
                    }

                    // one file
                    bool setIndex = false;
                    if (allSingleFiles == null)
                    {
                        setIndex = true;
                        try
                        {
                            allSingleFiles = GetFiles(Path.GetDirectoryName(this.startPath));
                        }
                        catch { }
                    }
                    if ((allSingleFiles == null) || (allSingleFiles.Length <= 1))
                    {
                        return false;
                    }
                    
                    if (setIndex) 
                    {
                        this.allSingleFilesIndex = FindFile(this.startPath);
                        if (this.allSingleFilesIndex < 0) this.allSingleFilesIndex = 0;
                    }
                    newIndex = this.allSingleFilesIndex;

                    switch (direction)
                    {
                        case MoveDirection.PageDown:
                        case MoveDirection.Down:
                            newIndex++;
                            break;
                        case MoveDirection.PageUp:
                        case MoveDirection.Up:
                            newIndex--;
                            break;
                        case MoveDirection.Head:
                            newIndex = 0;
                            break;
                        case MoveDirection.End:
                            newIndex = -1;
                            break;
                    }
                    if (newIndex >= allSingleFiles.Length)
                    {
                        if (!canLoop)
                        {
                            return false;
                        }
                        newIndex = 0;
                    }
                    if (newIndex < 0)
                    {
                        if (!canLoop)
                        {
                            return false;
                        }
                        newIndex = allSingleFiles.Length - 1;
                    }
                    if (newIndex == this.allSingleFilesIndex)
                    {
                        return false;
                    }
                    if (this.startPath.ToLower() == allSingleFiles[newIndex].ToLower())
                    {
                        return false;
                    }
                    this.startPath = allSingleFiles[newIndex];
                    this.allSingleFilesIndex = newIndex;
                    return true;
                }
                catch { }
                finally
                {
                    withinMoveNext = false;
                }
            }
            return false;
        }

        private int FindFile(string file)
        {
            string t = file.ToLower();
            for (int i = 0; i < allSingleFiles.Length; i++)
            {
                if (allSingleFiles[i].ToLower() == t)
                {
                    return i;
                }
            }
            return -1;
        }

        #endregion move

        #region properties

        public int Count
        {
            get 
            {
                lock (lockObj)
                {
                    if (mfiles == null) return 0;
                    return mfiles.Length;
                }
            }
        }

        public Point RowCols
        {
            get
            {
                lock (lockObj) 
                {
                    return new Point(rows, cols);
                }
            }
        }

        public int CurrentIndex
        {
            get
            {
                lock (lockObj)
                {
                    return allSingleFilesIndex;
                }
            }
        }

        public string GetFileAt(int idx) 
        {
            if (idx < 0) return null;
            lock (lockObj)
            {
                if (!allSingleFilesInited) return null;
                if (allSingleFiles == null) return null;
                if (idx >= allSingleFiles.Length) return null;
                return allSingleFiles[idx];
            }
        }

        public MFile GetAt(int idx)
        {
            if (idx < 0) return null;
            lock (lockObj)
            {
                if (mfiles == null) return null;
                if (idx >= mfiles.Length) return null;
                return mfiles[idx];
            }
        }

        #endregion properties

    }//EOC
}
