using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TwoTrails;
using TwoTrails.DAL;
using TwoTrails.Core;
using System.IO;
using TwoTrails.Core.Points;
using System.Collections.Generic;
using CSUtil;

namespace TwoTrailsTest
{
    [TestClass]
    public class UnitTest1
    {
        private static string FILE = @"testFile.tt";

        [TestMethod]
        public void TestMethod1()
        {
            TtSettings settings = new TtSettings(new DeviceSettings(), new MetadataSettings());
            TtProjectInfo info = settings.CreateProjectInfo(AppInfo.Version);

            File.Delete(FILE);

            TtSqliteDataAccessLayer dal = TtSqliteDataAccessLayer.Create(FILE, info);

            TtManager manager = new TtManager(dal, settings);


            TtPolygon poly = manager.CreatePolygon();
            manager.AddPolygon(poly);

            TtMetadata meta = manager.DefaultMetadata;
            TtGroup group = manager.MainGroup;

            TtPoint point = new GpsPoint()
            {
                Index = 0,
                PID = 1010,
                OnBoundary = true,
                UnAdjX = 100,
                UnAdjY = 100,
                UnAdjZ = 5,
                Latitude = 100,
                Longitude = 200,
                Elevation = 25,
                Metadata = meta,
                Polygon = poly,
                Group = group
            };

            List<TtPoint> points = new List<TtPoint>();

            points.Add(point);

            point = new SideShotPoint()
            {
                Index = 1,
                PID = 1030,
                OnBoundary = true,
                UnAdjX = 1,
                UnAdjY = 2,
                UnAdjZ = 3,
                FwdAzimuth = 180,
                SlopeDistance = 25,
                Metadata = meta,
                Polygon = poly,
                Group = group
            };

            points.Add(point);

            point = new QuondamPoint()
            {
                Index = 2,
                PID = 1040,
                OnBoundary = true,
                UnAdjX = 1,
                UnAdjY = 2,
                UnAdjZ = 3,
                ParentPoint = points[0],
                Metadata = meta,
                Polygon = poly,
                Group = group
            };

            points.Add(point);


            manager.AddPoints(points);

            manager.Save();

            Console.WriteLine(points.ToStringContents(p => p.PID.ToString(), "\n"));
        }
    }
}
