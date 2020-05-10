﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using SGP_Ephemerides.Support;

namespace SGP_Ephemerides.Charts
{
    public class AltitudeChart
    {
        public Chart mChart { get; set; }
        private List<ChartArea> mChartAreaList;

        public Location.Location Location { get; set; }
        public bool Legend { set { mLegend.Enabled = value; } }

        private ChartArea mChartArea;
        private List<Target.Target> mTargetList;
        private Target.Target mTarget;
        private List<Series> mSeriesList;
        private Series mSeries;
        private Legend mLegend;
        private UIState mUIState;

        private AltitudeSeries mAltitudeSeries;

        //################################################################################################################
        //################################################################################################################

        public AltitudeChart()
        {
            mChart = new Chart();
            mChartAreaList = new List<ChartArea>();
            //mChart.ChartAreas.Add(mChartArea);
            mTargetList = new List<Target.Target>();
            mTarget = new Target.Target();
            mSeriesList = new List<Series>();
            mSeries = new Series();
            mLegend = new Legend();
            mAltitudeSeries = new AltitudeSeries();
            mUIState = new Support.UIState();

            mChart.MouseClick += new MouseEventHandler(this.Chart_MouseClick);
        }

        private void Chart_MouseClick(object sender, MouseEventArgs e)
        {
            HitTestResult result = mChart.HitTest(e.X, e.Y);

            if (result != null && result.Object != null && result.Object is LegendItem && e.Button == MouseButtons.Left)
            {
                LegendItem legendItem = (LegendItem)result.Object;

                Series series = mChart.Series[legendItem.SeriesName];

                if (series.Color == Color.Empty)
                {
                    series.Color = Color.Transparent;
                }
                else
                {
                    series.Color = Color.Empty;
                }
            }
        }

        //################################################################################################################
        //################################################################################################################
        public void UIState(Support.UIState state)
        {
            mUIState = state;
        }

        public void ClearChartAreaList()
        {
            mChartAreaList.Clear();
        }

        public void AddChartAreaToList(string chartAreaName)
        {
            mChartArea = new ChartArea(chartAreaName);
            mChartArea.BackColor = Color.FromArgb(255, 70, 70, 70);

            if (mChartAreaList.Count >= 1)
            {
                mChartArea.AlignmentOrientation = mChartAreaList[0].AlignmentOrientation;
                mChartArea.Visible = false;
            }
            mChartAreaList.Add(mChartArea);
        }

        private void AddChartAreaToChart(string chartAreaName)
        {
            mChart.ChartAreas.Clear();

            foreach (ChartArea chartArea in mChartAreaList)
            {
                chartArea.Visible = false;

                if (chartArea.Name == chartAreaName)
                {
                    mChartArea = chartArea;
                }
            }

            mChart.ChartAreas.Add(mChartArea);
        }

        public void ShowChartAreaSeries(string chartAreaName)
        {
            AddChartAreaToChart(chartAreaName);

            AddHorizonLine(chartAreaName);

            ClearSeries();

            foreach (Series series in mAltitudeSeries.TargetSeriesList)
            {
                if (series.Name.Contains(chartAreaName))
                {
                    series.Enabled = true;
                    series.LegendText = series.Name.Remove(series.Name.IndexOf("-"));
                    AddSeries(series);
                }
                else
                {
                    series.Enabled = false;
                }
            }

            SetChartAreaAxis(chartAreaName);
            mChart.ChartAreas[chartAreaName].Visible = true;
        }

        public void BuildTargetSeriesList()
        {
            mAltitudeSeries.Location = Location;

            foreach (Target.Target target in mTargetList)
            {
                mAltitudeSeries.Target = target;
                mAltitudeSeries.BuildSeriesList();
            }
        }

        public string ChartTitle
        {
            set
            {
                mChart.Titles.Clear();
                mChart.Titles.Add(value);
            }
        }

        public void ClearSeries()
        {
            mChart.Series.Clear();
        }

        public void RemoveSeries(Series series)
        {
            mChart.Series.Remove(series);
        }

        public void AddSeries(Series series)
        {
            mChart.Series.Add(series);
        }

        public void ClearTargetList()
        {
            ClearSeries();
            mTargetList.Clear();
            mAltitudeSeries.ClearTargetList();
        }

        public void RemoveFromTargetList(Target.Target target)
        {

        }

        public void AddToTargetList(Target.Target target)
        {
            mTargetList.Add(target);
        }

        public void AddToTargetList(List<Target.Target> targetList)
        {
            foreach (Target.Target target in targetList)
            {
                AddToTargetList(target);
            }
        }

        public void SetChartAreaAxis(string chartAreaName)
        {
            foreach (Series series in mChart.Series)
            {
                series.Enabled = false;
            }

            switch (chartAreaName)
            {
                case "Day":
                case "Moon":
                    mChart.ChartAreas[chartAreaName].AxisX.Interval = 60.0;
                    mChart.ChartAreas[chartAreaName].AxisX.IntervalType = DateTimeIntervalType.Minutes;
                    mChart.ChartAreas[chartAreaName].AxisX.LabelStyle.Format = "h:mm tt";
                    mChart.ChartAreas[chartAreaName].AxisX.Title = "";

                    mChart.ChartAreas[chartAreaName].AxisY.Interval = 10;
                    mChart.ChartAreas[chartAreaName].AxisY.Maximum = 90.0;
                    mChart.ChartAreas[chartAreaName].AxisY.Minimum = 10.0;
                    mChart.ChartAreas[chartAreaName].AxisY.Title = "Maximum Altitude";

                    AddDawnDuskGradient(chartAreaName);
                    break;

                case "Year":
                    mChart.ChartAreas[chartAreaName].AxisX.Interval = 1.0;
                    mChart.ChartAreas[chartAreaName].AxisX.IntervalType = DateTimeIntervalType.Months;
                    mChart.ChartAreas[chartAreaName].AxisX.LabelStyle.Format = "MMMM";
                    mChart.ChartAreas[chartAreaName].AxisX.Title = "";

                    mChart.ChartAreas[chartAreaName].AxisY.Interval = 10;
                    mChart.ChartAreas[chartAreaName].AxisY.Maximum = 90.0;
                    mChart.ChartAreas[chartAreaName].AxisY.Minimum = 10.0;
                    mChart.ChartAreas[chartAreaName].AxisY.Title = "Maximum Daily Altitude";
                    break;

                case "Optimal":
                    mChart.ChartAreas[chartAreaName].AxisX.Interval = 1.0;
                    mChart.ChartAreas[chartAreaName].AxisX.IntervalType = DateTimeIntervalType.Months;
                    mChart.ChartAreas[chartAreaName].AxisX.LabelStyle.Format = "MMMM";
                    mChart.ChartAreas[chartAreaName].AxisX.Title = "";

                    mChart.ChartAreas[chartAreaName].AxisY.Interval = 10;
                    mChart.ChartAreas[chartAreaName].AxisY.Maximum = 90.0;
                    mChart.ChartAreas[chartAreaName].AxisY.Minimum = 10.0;
                    mChart.ChartAreas[chartAreaName].AxisY.Title = "Maximum Optimal Hourly Altitude";
                    break;

                default:
                    break;
            }

            foreach (Series series in mChart.Series)
            {
                if (series.Name.Contains(chartAreaName))
                {
                    series.Enabled = true;
                }
                else
                {
                    series.Enabled = false;
                }
            }

            mChart.Invalidate();
        }

        public void AddHorizonLine(string chartAreaName)
        {
            StripLine stripline = new StripLine();
            stripline.Interval = 0;
            stripline.IntervalOffset = Location.Horizon - 1;
            stripline.StripWidth = 2;
            stripline.BackColor = Color.Green;
            mChart.ChartAreas[chartAreaName].AxisY.StripLines.Add(stripline);
        }

        public void AddDawnDuskGradient(string chartAreaName)
        {
            double duskOffset;
            double dawnOffset;
            TimeSpan delta;
            StripLine stripLine;
            DateTime start;
            DateTime stop;

            

            stripLine = new StripLine();

            duskOffset = (Astrometry.AstronomicalDusk.Minute > 30.0) ? 0.0 : -1.0;
            start = Astrometry.AstronomicalDusk.AddHours(duskOffset).Date.AddHours(Astrometry.AstronomicalDusk.AddHours(duskOffset).Hour);
            stop = Astrometry.AstronomicalDusk;

            delta = stop.Subtract(start);

            stripLine.BackColor = Color.FromArgb(125, 80, 230, 210);
            stripLine.BackColor = Color.FromArgb(145, 255, 238, 88);
            stripLine.BackSecondaryColor = Color.FromArgb(255, 90, 90, 90);
            stripLine.BackGradientStyle = GradientStyle.LeftRight;
            stripLine.IntervalOffset = start.Minute;
            stripLine.IntervalOffsetType = DateTimeIntervalType.Minutes;
            stripLine.Interval = 0;
            stripLine.IntervalType = DateTimeIntervalType.Minutes;

            stripLine.StripWidth = delta.TotalMinutes;

            mChart.ChartAreas[chartAreaName].AxisX.StripLines.Add(stripLine);



            stripLine = new StripLine();

            dawnOffset = (Astrometry.AstronomicalDawn.Minute > 30.0) ? 2.0 : 1.0;
            start = Astrometry.AstronomicalDawn;
            stop = Astrometry.AstronomicalDawn.AddHours(dawnOffset).Date.AddHours(Astrometry.AstronomicalDawn.AddHours(dawnOffset).Hour);
            delta = stop.Subtract(start);

            stripLine.BackSecondaryColor = Color.FromArgb(145, 255, 238, 88);
            stripLine.BackColor = Color.FromArgb(255, 90, 90, 90);
            stripLine.BackGradientStyle = GradientStyle.LeftRight;
            stripLine.IntervalOffsetType = DateTimeIntervalType.Minutes;
            stripLine.Interval = 0;
            stripLine.IntervalType = DateTimeIntervalType.Minutes;
            stripLine.StripWidth = delta.TotalMinutes;

            start = Astrometry.AstronomicalDusk.AddHours(duskOffset).Date.AddHours(Astrometry.AstronomicalDusk.AddHours(duskOffset).Hour);
            delta = Astrometry.AstronomicalDawn.Subtract(start);

            stripLine.IntervalOffset = delta.TotalMinutes + 2;

            mChart.ChartAreas[chartAreaName].AxisX.StripLines.Add(stripLine);
        }

        public void AddLegend()
        {
            mChart.Legends.Add(mLegend);
        }
        public void ClearLegend()
        {
            mChart.Legends.Clear();
        }
    }
}
