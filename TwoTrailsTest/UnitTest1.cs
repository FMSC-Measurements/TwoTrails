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

            List<TtPoint> points = new List<TtPoint>();

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

            points.Add(point);

            point = new GpsPoint()
            {
                Index = 1,
                PID = 1020,
                OnBoundary = true,
                UnAdjX = 100,
                UnAdjY = 200,
                UnAdjZ = 5,
                Latitude = 100,
                Longitude = 200,
                Elevation = 25,
                Metadata = meta,
                Polygon = poly,
                Group = group
            };

            points.Add(point);

            point = new GpsPoint()
            {
                Index = 2,
                PID = 1030,
                OnBoundary = true,
                UnAdjX = 200,
                UnAdjY = 200,
                UnAdjZ = 5,
                Latitude = 100,
                Longitude = 200,
                Elevation = 25,
                Metadata = meta,
                Polygon = poly,
                Group = group
            };

            points.Add(point);

            point = new GpsPoint()
            {
                Index = 3,
                PID = 1040,
                OnBoundary = true,
                UnAdjX = 200,
                UnAdjY = 100,
                UnAdjZ = 5,
                Latitude = 100,
                Longitude = 200,
                Elevation = 25,
                Metadata = meta,
                Polygon = poly,
                Group = group
            };

            points.Add(point);

            point = new QuondamPoint()
            {
                Index = 4,
                PID = 1050,
                OnBoundary = true,
                UnAdjX = 100,
                UnAdjY = 100,
                UnAdjZ = 5,
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
