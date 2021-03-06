﻿using System;
using System.IO;

namespace TwoTrails.Core.Media
{
    public abstract class TtMedia : TtObject
    {
        private String _Name;
        public String Name
        {
            get { return _Name; }
            set
            {
                SetField(ref _Name, value);
            }
        }

        private String _FilePath;
        public String FilePath
        {
            get { return _FilePath; }
            set
            {
                SetField(ref _FilePath, value);
            }
        }

        private String _Comment;
        public String Comment
        {
            get { return _Comment; }
            set
            {
                SetField(ref _Comment, value);
            }
        }

        private DateTime _TimeCreated;
        public DateTime TimeCreated
        {
            get { return _TimeCreated; }
            set
            {
                SetField(ref _TimeCreated, value);
            }
        }

        private String _PointCN;
        public String PointCN
        {
            get { return _PointCN; }
            set
            {
                SetField(ref _PointCN, value);
            }
        }

        private bool _IsExternal;
        public bool IsExternal
        {
            get { return _IsExternal; }
            protected set
            {
                SetField(ref _IsExternal, value);
            }
        }
        

        public TtMedia() { }

        public TtMedia(String cn, String name, String filePath, String comment, DateTime timeCreated, String pointCN, bool isExternal)
            : base(cn)
        {
            this._Name = name;
            this._FilePath = filePath;
            this._Comment = comment;
            this._TimeCreated = timeCreated;
            this._PointCN = pointCN;
            this._IsExternal = isExternal;
        }

        public TtMedia(TtMedia media) : base(media.CN)
        {
            this._Name = media._Name;
            this._FilePath = media._FilePath;
            this._Comment = media._Comment;
            this._TimeCreated = media._TimeCreated;
            this._PointCN = media._PointCN;
            this._IsExternal = media._IsExternal;
        }


        public abstract MediaType MediaType { get; }

        public bool ExternalFileExists { get { return File.Exists(_FilePath); } }
    }
}
