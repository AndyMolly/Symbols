using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using CMA.MICAPS.GMap;
using CMA.MICAPS.GMap.Data;
using CMA.MICAPS.GMap.Data.Providers;
using CMA.MICAPS.Infrastructures.FileSystems;
using CMA.MICAPS.Services;

namespace CMA.MICAPS.Providers
{
    class Diamond4DataProvider : IGridDataProvider, ICloneable
    {
        private OpenedFileInfo _file = null;
        private Envelope _bounds = new Envelope(Coordinate.Zero, Coordinate.Zero);
        private List<GridData> _grid_datas = null;
        private float _xInteravl, _yInterval;
        private MetaData _meta = new MetaData();
        private bool _IsLoaded = false;

        public Diamond4DataProvider(string uri) :
            this(new FSPath(uri))
        {
        }

        public Diamond4DataProvider(FSPath path) :
            this(new OpenedFileInfo(path))
        {
        }

        public Diamond4DataProvider(OpenedFileInfo file)
        {
            _file = file;
            this.Name = FSPath.GetName(_file.FilePath);
        }

        internal Diamond4DataProvider()
        {

        }

        #region IGridDataProvider
        public void Load()
        {
            char[] seperator = new char[] { ' ', '\r', '\n', '\t' };
            Stream stream = _file.OpenRead();
            Encoding encoding = Encoding.Default;
            using (StreamReader sr = new StreamReader(stream, encoding))
            {
                string contents = sr.ReadToEnd();
                string[] subContents = contents.Split(seperator, StringSplitOptions.RemoveEmptyEntries);

                this.Name = subContents[2];

                //read headers 
                int year = int.Parse(subContents[3]);
                int month = int.Parse(subContents[4]);
                int day = int.Parse(subContents[5]);
                int hour = int.Parse(subContents[6]);

                DateTime createTime = new DateTime(year, month, day, hour, 0, 0);

                int xSize = int.Parse(subContents[15]);
                int ySize = int.Parse(subContents[16]);
                _xInteravl = float.Parse(subContents[9]);
                _yInterval = float.Parse(subContents[10]);

                _meta.SetValue("default", "description", subContents[2]);
                _meta.SetValue("default", "year", subContents[3]);
                _meta.SetValue("default", "month", subContents[4]);
                _meta.SetValue("default", "day", subContents[5]);
                _meta.SetValue("default", "hour", subContents[6]);
                _meta.SetValue("default", "duration", subContents[7]);
                _meta.SetValue("default", "level", subContents[8]);
                _meta.SetValue("default", "xinteravl", subContents[9]);
                _meta.SetValue("default", "yinteravl", subContents[10]);
                _meta.SetValue("default", "startlon", subContents[11]);
                _meta.SetValue("default", "endlon", subContents[12]);
                _meta.SetValue("default", "startlat", subContents[13]);
                _meta.SetValue("default", "endlat", subContents[14]);
                _meta.SetValue("default", "xsize", subContents[15]);
                _meta.SetValue("default", "ysize", subContents[16]);
                _meta.SetValue("default", "lineinteravl", subContents[17]);
                _meta.SetValue("default", "startvalue", subContents[18]);
                _meta.SetValue("default", "endvalue", subContents[19]);
                _meta.SetValue("default", "smooth", subContents[20]);
                //_meta.SetValue("default", "boldvalue", subContents[21]);

                float lineInteravl = Convert.ToSingle(subContents[17]);
                float startValue = Convert.ToSingle(subContents[18]);
                float endValue = Convert.ToSingle(subContents[19]);

                float startLon = Convert.ToSingle(subContents[11]);
                float endLon = Convert.ToSingle(subContents[12]);
                float startLat = Convert.ToSingle(subContents[13]);
                float endLat = Convert.ToSingle(subContents[14]);

                Coordinate min = new Coordinate(startLon < endLon ? startLon : endLon,
                    startLat < endLat ? startLat : endLat, 0);
                Coordinate max = new Coordinate(endLon > startLon ? endLon : startLon,
                    endLat > startLat ? endLat : startLat, 0);
                _bounds.SetExtents(min, max);

                GridData gridData = new GridData(xSize, ySize, GridDataType.Float32);
                ReadRecord(gridData, subContents);
                gridData.NoDataValue = 9999F;

                if (endValue < startValue && lineInteravl > 0)
                {
                    float tmpv = endValue;
                    endValue = startValue;
                    startValue = tmpv;
                }
                if (startValue < gridData.MinValue)
                {
                    int itt;
                    itt = (int)(Math.Abs((gridData.MinValue - startValue) / lineInteravl));
                    startValue = startValue + itt * lineInteravl;
                }
                if (endValue > gridData.MaxValue)
                {
                    int itt;
                    itt = (int)(Math.Abs((gridData.MaxValue - endValue) / lineInteravl));
                    endValue = endValue - itt * lineInteravl;
                }

                if (endValue < startValue && lineInteravl > 0)
                {
                    float tmpv = endValue;
                    endValue = startValue;
                    startValue = tmpv;
                }

                StringBuilder sb = new StringBuilder();
                if (lineInteravl != 0)
                {
                    for (float i = startValue; i <= endValue; i = i + lineInteravl)
                    {
                        sb.Append(i.ToString()).Append(',');
                    }
                }
                _meta.SetValue("default", "analysisvalues", sb.ToString().TrimEnd(','));

            }
            _IsLoaded = true;
        }

        public Envelope Extents
        {
            get { return _bounds; }
            set { _bounds = value; }
        }

        public void Dispose()
        {
            if (_file != null)
                _file.Dispose();
        }

        public IFeatureProvider GetFeatureProvider(int band, bool force = false)
        {
            return null;
        }

        public GridData GetGrid(int band)
        {
            if (band >= Count)
                band = Count - 1;
            return _grid_datas[band];
        }

        public int Count
        {
            get { return _grid_datas.Count; }
        }

        public int XSize
        {
            get { return _grid_datas[0].XSize; }
        }

        public int YSize
        {
            get { return _grid_datas[0].YSize; }
        }

        public string Name
        {
            get;
            set;
        }

        public string[] GetMetaData(string domain)
        {
            return _meta.GetDomainValues(domain);
        }

        public string GetMetaValue(string domain, string name)
        {
            return _meta.GetValue(domain, name);
        }

        public void SetMetaValue(string domain, string name, string value)
        {
            _meta.SetValue(domain, name, value);
        }

        public double XInterval
        {
            get { return _xInteravl; }
            set { _xInteravl = (float)value; }
        }

        public double YInterval
        {
            get { return _yInterval; }
            set { _yInterval = (float)value; }
        }

        public string Uri
        {
            get { return _file.FilePath.GetUri().OriginalString; }
        }

        public bool IsLoaded
        {
            get { return _IsLoaded; }
        }

        public TimeSpan Duration
        {
            get { throw new NotImplementedException(); }
        }

        public TimeSpan Step
        {
            get { throw new NotImplementedException(); }
        }

        public DateTime Timestamp
        {
            get { throw new NotImplementedException(); }
        }
        #endregion

        #region Private Func
        private void ReadRecord(GridData griddata, string[] subContents)
        {
            if (_grid_datas == null)
                _grid_datas = new List<GridData>();

            float maxV = float.MinValue;
            float minV = float.MaxValue;

            float[] data = new float[griddata.XSize * griddata.YSize];
            for (int i = 0; i < data.Length; i++)
            {
                float v = float.Parse(subContents[22 + i]);
                data[i] = v;
                if (v == 9999f)
                    continue;
                if (v > maxV)
                    maxV = data[i];
                else if (v < minV)
                    minV = data[i];
            }
            griddata.MaxValue = maxV;
            griddata.MinValue = minV;

            IntPtr ptr = griddata.Lock();
            Marshal.Copy(data, 0, ptr, data.Length);

            _grid_datas.Add(griddata);
            griddata.Unlock();
        }
        #endregion

        public object Clone()
        {
            Diamond4DataProvider new_provider = new Diamond4DataProvider();
            new_provider.Name = this.Name;
            new_provider.XInterval = this.XInterval;
            new_provider.YInterval = this.YInterval;
            new_provider._meta = this._meta;
            new_provider.Extents = new Envelope(Coordinate.Zero, Coordinate.Zero);
            new_provider.Extents.SetExtents(this.Extents.Minimum, this.Extents.Maximum);

            new_provider._grid_datas = new List<GridData>();
            for (int i = 0; i < Count; i++)
            {
                GridData gridData = _grid_datas[i];

                GridData newData = new GridData(XSize, YSize, GridDataType.Float32);
                newData.MaxValue = gridData.MaxValue;
                newData.MinValue = gridData.MinValue;
                newData.NoDataValue = gridData.NoDataValue;

                float[] data = new float[gridData.XSize * gridData.YSize];
                for (int j = 0; j < data.Length; j++)
                {
                    data[j] = gridData.RawData[j];
                }

                IntPtr ptr = newData.Lock();
                Marshal.Copy(data, 0, ptr, data.Length);
                new_provider._grid_datas.Add(newData);
                newData.Unlock();
            }

            return new_provider;
        }
    }
}
