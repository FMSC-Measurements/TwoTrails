using CSUtil.ComponentModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using TwoTrails.Core;
using TwoTrails.Core.Points;

namespace TwoTrails.Controls
{
    public class DataEditorModel : NotifyPropertyChangedEx
    {
        private ICollectionView _Points;
        public ICollectionView Points
        {
            get { return _Points; }
            set { SetField(ref _Points, value); }
        }

        ITtManager _Manager;


        public bool IsAdvancedMode { get { return Get<bool>(); } set { Set(value); } }

        public bool HasSelection
        {
            get { return _SelectedPoints.Count > 0; }
        }

        public bool MultipleSelections
        {
            get { return _SelectedPoints.Count > 1; }
        }



        private List<TtPoint> _AllPoints;
        private List<TtPoint> _VisiblePoints;


        private TtPoint _SelectedPoint;
        public TtPoint SelectedPoint
        {
            get { return _SelectedPoint; }
        }

        private List<TtPoint> _SelectedPoints = new List<TtPoint>();
        public List<TtPoint> SelectedPoints
        {
            get { return _SelectedPoints; }
        }



        private bool _HasGps, _HasTrav, _HasQndm;

        public bool HasGps { get { return _HasGps; } }
        public bool HasTrav { get { return _HasTrav; } }
        public bool HasQndm { get { return _HasQndm; } }

        private bool _EnableTrav;
        public bool EnableTrav { get { return _EnableTrav; } }

        private bool _EnableManAcc;
        public bool EnableManAcc { get { return _EnableManAcc; } }

        private bool _EnableXYZ;
        public bool EnableXYZ { get { return _EnableXYZ; } }

        private bool _EnableQndm;
        public bool EnableQndm{ get { return _EnableQndm; } }


        private void OnSelectionChanged()
        {
            if (HasSelection)
            {
                if (MultipleSelections)
                {
                    TtPoint fpt = SelectedPoints[0];

                    TtPolygon poly = fpt.Polygon;
                    TtMetadata meta = fpt.Metadata;
                    TtGroup group = fpt.Group;

                    double? unAdjX = null,
                        unAdjY = null,
                        unAdjZ = null,
                        fwdAz = null,
                        bkAz = null,
                        slpDist = null,
                        slpAng = null,
                        manacc = null;

                    bool sameUnAdjX = true, sameUnAdjY = true, sameUnAdjZ = true, 
                        sameFwAz = true, sameBkAz = true, sameManAcc = true,
                        sameSlpDist = true, sameSlpAng = true;

                    _HasGps = false;
                    _HasTrav = false;
                    _HasQndm = false;

                    bool hasOnbnd = false, hasOffBnd = false;

                    bool sameCmt = true;
                    string cmt = fpt.Comment;

                    bool parseTrav = true;
                    bool parseManAcc = true;
                    bool parseQndm = true;

                    TtPoint qParent = null;
                    bool sameQParent = true;

                    foreach (TtPoint point in SelectedPoints)
                    {
                        if (poly != null && poly.CN != point.PolygonCN)
                            poly = null;

                        if (meta != null && meta.CN != point.MetadataCN)
                            meta = null;

                        if (group != null && group.CN != point.GroupCN)
                            group = null;

                        if (point.OnBoundary)
                        {
                            if (!hasOnbnd)
                                hasOnbnd = true;
                        }
                        else
                        {
                            if (!hasOffBnd)
                                hasOffBnd = true;
                        }


                        if (point.IsGpsType() && !_HasGps)
                        {
                            _HasGps = true;
                            
                            parseTrav = false;
                            parseQndm = false;
                        }
                        else if (point.IsTravType() && !_HasTrav)
                        {
                            _HasTrav = true;
                            
                            parseManAcc = false;
                            parseQndm = false;
                        }
                        else if (point.OpType == OpType.Quondam && !_HasQndm)
                        {
                            parseTrav = false;
                        }


                        if (sameCmt)
                        {
                            CompareAndSet(ref sameCmt, ref cmt, point.Comment);
                        }

                        if (sameUnAdjX)
                        {
                            CompareAndSet(ref sameUnAdjX, ref unAdjX, point.UnAdjX);
                        }

                        if (sameUnAdjY)
                        {
                            CompareAndSet(ref sameUnAdjY, ref unAdjY, point.UnAdjY);
                        }

                        if (sameUnAdjZ)
                        {
                            CompareAndSet(ref sameUnAdjZ, ref unAdjZ, point.UnAdjZ);
                        }


                        if (parseTrav && point.IsTravType())
                        {
                            TravPoint tp = point as TravPoint;

                            if (sameFwAz && fwdAz != tp.FwdAzimuth)
                            {
                                sameFwAz = false;
                            }

                            if (sameBkAz && bkAz != tp.BkAzimuth)
                            {
                                sameBkAz = false;
                            }

                            if (sameSlpAng && slpAng != tp.SlopeAngle)
                            {
                                sameSlpAng = false;
                            }

                            if (sameSlpDist && slpDist != tp.SlopeDistance)
                            {
                                sameSlpDist = false;
                            }
                        }

                        if (parseManAcc && (point.IsGpsType() || point.OpType == OpType.Quondam))
                        {
                            IManualAccuracy ima = point as IManualAccuracy;
                            
                            if (sameManAcc)
                            {
                                if (manacc != null && manacc != ima.ManualAccuracy)
                                {
                                    sameManAcc = false; 
                                }
                                else
                                {
                                    manacc = ima.ManualAccuracy;
                                }
                            }

                            if (parseQndm && point.OpType == OpType.Quondam)
                            {
                                QuondamPoint qp = point as QuondamPoint;

                                if (sameQParent)
                                {
                                    if (qParent != null && qp.ParentPointCN != qParent.CN)
                                    {
                                        sameQParent = false; 
                                    }
                                    else
                                    {
                                        qParent = qp.ParentPoint;
                                    }
                                }
                            }
                        }
                    }


                    _PID = null;
                    _SamePID = false;

                    _Index = null;
                    _SameIndex = false;

                    _Comment = cmt;
                    _SameComment = sameCmt;

                    _Polygon = poly;
                    _SamePolygons = poly != null;

                    _Metadata = meta;
                    _SameMetadata = meta != null;

                    _Group = group;
                    _SameGroup = group != null;

                    if (hasOnbnd ^ hasOffBnd)
                    {
                        _OnBound = hasOnbnd;
                        _SameOnBound = true;
                    }
                    else
                    {
                        _OnBound = null;
                        _SameOnBound = true;
                    }

                    _ManAcc = parseManAcc ? manacc : null;
                    _SameManAcc = parseManAcc ? sameManAcc : true;
                    

                    if (parseTrav)
                    {
                        _FwdAz = sameFwAz ? fwdAz : null;
                        _SameFwdAz = sameFwAz;

                        _BkAz = sameBkAz ? bkAz : null;
                        _SameBkAz = sameBkAz;

                        _SlpAng = SameSlpAng ? slpAng : null;
                        _SameSlpAng = sameSlpAng;

                        _SlpDist = sameSlpDist ? SlpDist : null;
                        _SameSlpDist = sameSlpDist;
                    }
                    else
                    {
                        _FwdAz = null;
                        _SameFwdAz = true;

                        _BkAz = null;
                        _SameBkAz = true;

                        _SlpAng = null;
                        _SameSlpAng = true;

                        _SlpDist = null;
                        _SameSlpDist = true;
                    }

                    if (parseQndm)
                    {
                        _ParentPoint = qParent;
                        _SameParentPoint = sameQParent;
                    }
                    else
                    {
                        _ParentPoint = null;
                        _SameParentPoint = true;
                    }

                    _UnAdjX = sameUnAdjX ? unAdjX : null;
                    _SameUnAdjX = sameUnAdjX;

                    _UnAdjY = sameUnAdjY ? unAdjY : null;
                    _SameUnAdjY = sameUnAdjY;
                    
                    _UnAdjZ = sameUnAdjZ ? unAdjZ : null;
                    _SameUnAdjZ = sameUnAdjZ;
                }
                else
                {
                    _HasGps = false;
                    _HasTrav = false;
                    _HasQndm = false;

                    _PID = _SelectedPoint.PID;
                    _SamePID = true;

                    _Index = _SelectedPoint.Index;
                    _SameIndex = true;

                    _Comment = _SelectedPoint.Comment;
                    _SameComment = true;

                    _Polygon = _SelectedPoint.Polygon;
                    _SamePolygons = true;

                    _Metadata = _SelectedPoint.Metadata;
                    _SameMetadata = true;

                    _Group = _SelectedPoint.Group;
                    _SameGroup = true;
                    
                    _OnBound = _SelectedPoint.OnBoundary;
                    _SameOnBound = true;


                    if (_SelectedPoint.IsGpsType())
                    {
                        _HasGps = true;

                        _ManAcc = (_SelectedPoint as GpsPoint).ManualAccuracy;
                        _ParentPoint = null;
                    }
                    else if (_SelectedPoint.OpType == OpType.Quondam)
                    {
                        QuondamPoint qp = _SelectedPoint as QuondamPoint;

                        _HasQndm = true;

                        _ManAcc = qp.ManualAccuracy;
                        _ParentPoint = qp.ParentPoint;
                    }
                    else
                    {
                        _ManAcc = null;
                    }

                    _SameManAcc = true;
                    _SameParentPoint = true;


                    if (_SelectedPoint.OpType == OpType.Quondam)
                        _ParentPoint = (_SelectedPoint as QuondamPoint).ParentPoint;
                    else
                        _ParentPoint = null;

                    _SameParentPoint = true;


                    if (_SelectedPoint.IsTravType())
                    {
                        _HasTrav = true;

                        TravPoint tp = _SelectedPoint as TravPoint;

                        _FwdAz = tp.FwdAzimuth;
                        _SameFwdAz = true;

                        _BkAz = tp.BkAzimuth;
                        _SameBkAz = true;

                        _SlpAng = tp.SlopeAngle;
                        _SameSlpAng = true;

                        _SlpDist = tp.SlopeDistance;
                        _SameSlpDist = true;
                    }
                    else
                    {
                        _FwdAz = null;
                        _SameFwdAz = true;

                        _BkAz = null;
                        _SameBkAz = true;

                        _SlpAng = null;
                        _SameSlpAng = true;

                        _SlpDist = null;
                        _SameSlpDist = true;
                    }

                    _UnAdjX = _SelectedPoint.UnAdjX;
                    _SameUnAdjX = true;

                    _UnAdjY = _SelectedPoint.UnAdjY;
                    _SameUnAdjY = true;

                    _UnAdjZ = _SelectedPoint.UnAdjZ;
                    _SameUnAdjZ = true;
                } 
            }
            else
            {
                _PID = null;
                _SamePID = true;

                _Index = null;
                _SameIndex = true;

                _Comment = null;
                _SameComment = true;

                _Polygon = null;
                _SamePolygons = true;

                _Metadata = null;
                _SameMetadata = true;

                _Group = null;
                _SameGroup = true;

                _OnBound = false;
                _SameOnBound = true;
                
                _ManAcc = null;
                _SameManAcc = true;

                _FwdAz = null;
                _SameFwdAz = true;

                _BkAz = null;
                _SameBkAz = true;

                _SlpAng = null;
                _SameSlpAng = true;

                _SlpDist = null;
                _SameSlpDist = true;

                _ParentPoint = null;
                _SameParentPoint = true;

                _UnAdjX = null;
                _SameUnAdjX = true;

                _UnAdjY = null;
                _SameUnAdjY = true;

                _UnAdjZ = null;
                _SameUnAdjZ = true;
            }

            _EnableTrav = HasTrav && !HasGps && !HasQndm;
            _EnableManAcc = !HasTrav && HasGps || HasQndm;
            _EnableXYZ = !HasTrav && HasGps && !HasQndm;
            _EnableQndm = !HasTrav && !HasGps && HasQndm;

            OnPropertyChanged(
                    nameof(HasSelection),
                    nameof(SelectedPoint),
                    nameof(SelectedPoints),
                    nameof(MultipleSelections),
                    nameof(EnableXYZ),
                    nameof(EnableTrav),
                    nameof(EnableManAcc),
                    nameof(EnableQndm),
                    nameof(PID),
                    nameof(SamePID),
                    nameof(Index),
                    nameof(SameIndex),
                    nameof(Polygon),
                    nameof(SamePolygon),
                    nameof(Metadata),
                    nameof(SameMetadata),
                    nameof(Group),
                    nameof(SameGroup),
                    nameof(OnBound),
                    nameof(SameOnBound),
                    nameof(UnAdjX),
                    nameof(SameUnAdjX),
                    nameof(UnAdjY),
                    nameof(SameUnAdjY),
                    nameof(UnAdjZ),
                    nameof(SameUnAdjZ),
                    nameof(ManAcc),
                    nameof(SameManAcc),
                    nameof(FwdAz),
                    nameof(SameFwdAz),
                    nameof(BkAz),
                    nameof(SameBkAz),
                    nameof(SlpAng),
                    nameof(SameSlpAng),
                    nameof(SlpDist),
                    nameof(SameSlpDist),
                    nameof(ParentPoint),
                    nameof(SameParentPoint),
                    nameof(Comment),
                    nameof(SameComment)
                );
        }
        
        
        private void CompareAndSet<T>(ref bool same, ref T oldVal, T newVal)
        {
            if (oldVal != null && !EqualityComparer<T>.Default.Equals(oldVal, newVal))
            {
                same = false;
            }
            else
            {
                oldVal = newVal;
            }
        }


        private bool _SamePID;
        public bool SamePID { get { return _SamePID; } }

        private int? _PID;
        public int? PID
        {
            get { return _PID; }
            set
            {
                //TODO
            }
        }


        private bool _SamePolygons;
        public bool SamePolygon { get { return _SamePolygons; } }

        private TtPolygon _Polygon;
        public TtPolygon Polygon
        {
            get { return _Polygon; }
            set
            {
                //TODO
            }
        }


        private bool _SameIndex;
        public bool SameIndex { get { return _SameIndex; } }

        private int? _Index;
        public int? Index
        {
            get { return _Index; }
            set
            {
                //TODO
            }
        }

        private bool _SameComment;
        public bool SameComment { get { return _SameComment; } }

        private string _Comment;
        public string Comment
        {
            get { return _Comment; }
            set
            {
                //TODO
            }
        }


        private bool _SameMetadata;
        public bool SameMetadata
        {
            get { return _SameMetadata; }
        }

        private TtMetadata _Metadata;
        public TtMetadata Metadata
        {
            get { return _Metadata; }
            set
            {
                //TODO
            }
        }


        private bool _SameGroup;
        public bool SameGroup { get { return _SameGroup; } }

        private TtGroup _Group;
        public TtGroup Group
        {
            get { return _Group; }
            set
            {
                //TODO
            }
        }


        private bool _SameOnBound;
        public bool SameOnBound { get { return _SameOnBound; } }

        private bool? _OnBound;
        public bool? OnBound
        {
            get { return _SameOnBound; }
            set
            {
                //TODO
            }
        }

        private bool _SameUnAdjX;
        public bool SameUnAdjX { get { return _SameUnAdjX; } }

        private double? _UnAdjX;
        public double? UnAdjX
        {
            get { return _UnAdjX; }
            set
            {
                //TODO
            }
        }


        private bool _SameUnAdjY;
        public bool SameUnAdjY { get { return _SameUnAdjY; } }

        private double? _UnAdjY;
        public double? UnAdjY
        {
            get { return _UnAdjY; }
            set
            {
                //TODO
            }
        }


        private bool _SameUnAdjZ;
        public bool SameUnAdjZ { get { return _SameUnAdjZ; } }

        private double? _UnAdjZ;
        public double? UnAdjZ
        {
            get { return _UnAdjZ; }
            set
            {
                //TODO
            }
        }


        private bool _SameManAcc;
        public bool SameManAcc { get { return _SameManAcc; } }

        private double? _ManAcc;
        public double? ManAcc
        {
            get { return _ManAcc; }
            set
            {
                //TODO
            }
        }


        private bool _SameFwdAz;
        public bool SameFwdAz { get { return _SameFwdAz; } }

        private double? _FwdAz;
        public double? FwdAz
        {
            get { return _FwdAz; }
            set
            {
                //TODO
            }
        }



        private bool _SameBkAz;
        public bool SameBkAz { get { return _SameBkAz; } }

        private double? _BkAz;
        public double? BkAz
        {
            get { return _BkAz; }
            set
            {
                //TODO
            }
        }




        private bool _SameSlpAng;
        public bool SameSlpAng { get { return _SameSlpAng; } }

        private double? _SlpAng;
        public double? SlpAng
        {
            get { return _SlpAng; }
            set
            {
                //TODO
            }
        }



        private bool _SameSlpDist;
        public bool SameSlpDist { get { return _SameSlpDist; } }

        private double? _SlpDist;
        public double? SlpDist
        {
            get { return _SlpDist; }
            set
            {
                //TODO
            }
        }




        private bool _SameParentPoint;
        public bool SameParentPoint { get { return _SameParentPoint; } }

        private TtPoint _ParentPoint;
        public TtPoint ParentPoint
        {
            get { return _ParentPoint; }
            set
            {
                //TODO
            }
        }







        public DataEditorModel(TtProject project)
        {
            _Manager = project.Manager;
            _AllPoints = _Manager.GetPoints();

            _AllPoints.Sort();
            _VisiblePoints = _AllPoints;
            Points = CollectionViewSource.GetDefaultView(_VisiblePoints);
            
            //Points.Filter = Filter;


        }



        public void UpdatePoints(IEnumerable<TtPoint> addedPoints, IEnumerable<TtPoint> removedPoints)
        {
            foreach (TtPoint p in removedPoints)
            {
                _SelectedPoints.Remove(p);
            }

            foreach (TtPoint p in addedPoints)
            {
                _SelectedPoints.Add(p);
            }

            _SelectedPoint = _SelectedPoints.Count == 1 ? _SelectedPoints[0] : null;

            OnSelectionChanged();
        }



        private bool Filter(object obj)
        {
            TtPoint point = obj as TtPoint;
            return point.IsGpsAtBase();
        }

    }
}
