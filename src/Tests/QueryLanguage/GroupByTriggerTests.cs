using System;
using System.Collections.Generic;
using System.Linq;
using DbInterfaces.Interfaces;
using FileDb.InterfaceImpl;
using FluentAssertions;
using NUnit.Framework;
using QueryLanguage.Grouping.ByTrigger;

namespace Tests
{
    [TestFixture]
    public class GroupByTriggerTests
    {
        private List<ISingleDataRow<float>> _rows = new List<ISingleDataRow<float>>();
        private QuerySerie<float> _serie;
        private QuerySerie<float> _serie2;
        private QuerySerie<float> _serie3;

        [SetUp]
        public void Setup()
        {
            _rows.Clear();
            _rows.Add(new SingleDataRow<float>(new DateTime(1000, 1, 1, 0, 1, 0), 2));
            _rows.Add(new SingleDataRow<float>(new DateTime(1000, 1, 1, 0, 2, 0), 4));
            _rows.Add(new SingleDataRow<float>(new DateTime(1000, 1, 1, 0, 8, 0), 6));
            _rows.Add(new SingleDataRow<float>(new DateTime(1000, 1, 1, 0, 11, 0), 8));
            _rows.Add(new SingleDataRow<float>(new DateTime(1000, 1, 1, 0, 14, 0), 5));
            _rows.Add(new SingleDataRow<float>(new DateTime(1000, 1, 1, 0, 17, 0), 4));
            _rows.Add(new SingleDataRow<float>(new DateTime(1000, 1, 1, 0, 18, 0), 6));
            _rows.Add(new SingleDataRow<float>(new DateTime(1000, 1, 1, 0, 20, 0), 8));

            _serie = new QuerySerie<float>(_rows, new DateTime(1000, 1, 1), new DateTime(1000, 1, 1, 0, 22, 0))
            {
                PreviousRow = new SingleDataRow<float>(new DateTime(99, 1, 1, 0, 11, 0), 8),
                NextRow = new SingleDataRow<float>(new DateTime(1000, 1, 1, 0, 26, 0), 11)
            };

            _serie2 = new QuerySerie<float>(_rows.Take(6).ToList(), new DateTime(1000, 1, 1), new DateTime(1000, 1, 1, 0, 20, 0))
            {
                PreviousRow = new SingleDataRow<float>(new DateTime(99, 1, 1, 0, 11, 0), 8),
                NextRow = new SingleDataRow<float>(new DateTime(1000, 1, 1, 0, 26, 0), 11)
            };

            _serie3 = new QuerySerie<float>(_rows.Take(7).ToList(), new DateTime(1000, 1, 1), new DateTime(1000, 1, 1, 0, 20, 0))
            {
                PreviousRow = new SingleDataRow<float>(new DateTime(99, 1, 1, 0, 11, 0), 8),
                NextRow = new SingleDataRow<float>(new DateTime(1000, 1, 1, 0, 26, 0), 11)
            };
        }


        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void GroupTimesDefaultTest()
        {
            var times = _serie.GroupTimesByTrigger(c => c.TriggerWhen(i => i.Value > 5));

            times.Count.Should().Be(3);
            times[0].Start.Should().Be(new DateTime(1000, 1, 1, 0, 0, 0));
            times[0].End.Should().Be(new DateTime(1000, 1, 1, 0, 1, 0));
            times[1].Start.Should().Be(new DateTime(1000, 1, 1, 0, 8, 0));
            times[1].End.Should().Be(new DateTime(1000, 1, 1, 0, 14, 0));
            times[2].Start.Should().Be(new DateTime(1000, 1, 1, 0, 18, 0));
            times[2].End.Should().Be(new DateTime(1000, 1, 1, 0, 22, 0));
        }

        [Test]
        public void GroupTimesDefaultWithoutNextTest()
        {
            _serie.NextRow = null;
            var times = _serie.GroupTimesByTrigger(c => c.TriggerWhen(i => i.Value > 5));

            times.Count.Should().Be(3);
            times[0].Start.Should().Be(new DateTime(1000, 1, 1, 0, 0, 0));
            times[0].End.Should().Be(new DateTime(1000, 1, 1, 0, 1, 0));
            times[1].Start.Should().Be(new DateTime(1000, 1, 1, 0, 8, 0));
            times[1].End.Should().Be(new DateTime(1000, 1, 1, 0, 14, 0));
            times[2].Start.Should().Be(new DateTime(1000, 1, 1, 0, 18, 0));
            times[2].End.Should().Be(new DateTime(1000, 1, 1, 0, 20, 0));
        }

        [Test]
        public void GroupTimesDefault2WithoutNextTest()
        {
            _serie2.NextRow = null;
            var times = _serie2.GroupTimesByTrigger(c => c.TriggerWhen(i => i.Value > 5));

            times.Count.Should().Be(2);
            times[0].Start.Should().Be(new DateTime(1000, 1, 1, 0, 0, 0));
            times[0].End.Should().Be(new DateTime(1000, 1, 1, 0, 1, 0));
            times[1].Start.Should().Be(new DateTime(1000, 1, 1, 0, 8, 0));
            times[1].End.Should().Be(new DateTime(1000, 1, 1, 0, 14, 0));
        }

        [Test]
        public void GroupTimesDefault2Test()
        {
            var times = _serie2.GroupTimesByTrigger(c => c.TriggerWhen(i => i.Value > 5));

            times.Count.Should().Be(2);
            times[0].Start.Should().Be(new DateTime(1000, 1, 1, 0, 0, 0));
            times[0].End.Should().Be(new DateTime(1000, 1, 1, 0, 1, 0));
            times[1].Start.Should().Be(new DateTime(1000, 1, 1, 0, 8, 0));
            times[1].End.Should().Be(new DateTime(1000, 1, 1, 0, 14, 0));
        }

        [Test]
        public void GroupTimesDefaultOnly18Test()
        {
            _serie2.EndTime = new DateTime(1000, 1, 1, 0, 18, 0);
            var times = _serie2.GroupTimesByTrigger(c => c.TriggerWhen(i => i.Value > 5));

            times.Count.Should().Be(2);
            times[0].Start.Should().Be(new DateTime(1000, 1, 1, 0, 0, 0));
            times[0].End.Should().Be(new DateTime(1000, 1, 1, 0, 1, 0));
            times[1].Start.Should().Be(new DateTime(1000, 1, 1, 0, 8, 0));
            times[1].End.Should().Be(new DateTime(1000, 1, 1, 0, 14, 0));
        }

        [Test]
        public void GroupTimesDefaultEndIsTriggerTest()
        {
            _serie3.EndTime = new DateTime(1000, 1, 1, 0, 18, 0);
            var times = _serie3.GroupTimesByTrigger(c => c.TriggerWhen(i => i.Value > 5));

            times.Count.Should().Be(3);
            times[0].Start.Should().Be(new DateTime(1000, 1, 1, 0, 0, 0));
            times[0].End.Should().Be(new DateTime(1000, 1, 1, 0, 1, 0));
            times[1].Start.Should().Be(new DateTime(1000, 1, 1, 0, 8, 0));
            times[1].End.Should().Be(new DateTime(1000, 1, 1, 0, 14, 0));
            times[2].Start.Should().Be(new DateTime(1000, 1, 1, 0, 18, 0));
            times[2].End.Should().Be(new DateTime(1000, 1, 1, 0, 18, 0));
        }

        [Test]
        public void GroupTimesDefault_EndIsTrigger_NextIsNull_Test()
        {
            _serie3.EndTime = new DateTime(1000, 1, 1, 0, 18, 0);
            _serie3.NextRow = null;
            var times = _serie3.GroupTimesByTrigger(c => c.TriggerWhen(i => i.Value > 5));

            times.Count.Should().Be(3);
            times[0].Start.Should().Be(new DateTime(1000, 1, 1, 0, 0, 0));
            times[0].End.Should().Be(new DateTime(1000, 1, 1, 0, 1, 0));
            times[1].Start.Should().Be(new DateTime(1000, 1, 1, 0, 8, 0));
            times[1].End.Should().Be(new DateTime(1000, 1, 1, 0, 14, 0));
            times[2].Start.Should().Be(new DateTime(1000, 1, 1, 0, 18, 0));
            times[2].End.Should().Be(new DateTime(1000, 1, 1, 0, 18, 0));
        }

        [Test]
        public void GroupTimesOffsets_EndIsTrigger_NextIsNull_Test()
        {
            _serie3.EndTime = new DateTime(1000, 1, 1, 0, 18, 0);
            _serie3.NextRow = null;
            var times = _serie3.GroupTimesByTrigger(c => c.TriggerWhen(i => i.Value > 5)
                .StartOffset(TimeSpan.FromMinutes(-2)).EndOffset(TimeSpan.FromMinutes(3)));

            times.Count.Should().Be(3);
           // times[0].Start.Should().Be(new DateTime(1000, 1, 1, 0, 0, 0));
            times[0].End.Should().Be(new DateTime(1000, 1, 1, 0, 4, 0));
            times[1].Start.Should().Be(new DateTime(1000, 1, 1, 0, 6, 0));
            times[1].End.Should().Be(new DateTime(1000, 1, 1, 0, 17, 0));
            times[2].Start.Should().Be(new DateTime(1000, 1, 1, 0, 16, 0));
            times[2].End.Should().Be(new DateTime(1000, 1, 1, 0, 21, 0));
        }

        [Test]
        public void GroupTimesOffsets_EndIsStart_Test()
        {
            _serie3.EndTime = new DateTime(1000, 1, 1, 0, 18, 0);
            _serie3.NextRow = null;
            var times = _serie3.GroupTimesByTrigger(c => c.TriggerWhen(i => i.Value > 5).EndIsStart()
                .StartOffset(TimeSpan.FromMinutes(-2)).EndOffset(TimeSpan.FromMinutes(3)));

            times.Count.Should().Be(3);
            //times[0].Start.Should().Be(new DateTime(1000, 1, 1, 0, 0, 0));
            times[0].End.Should().Be(new DateTime(1000, 1, 1, 0, 3, 0));
            times[1].Start.Should().Be(new DateTime(1000, 1, 1, 0, 6, 0));
            times[1].End.Should().Be(new DateTime(1000, 1, 1, 0, 11, 0));
            times[2].Start.Should().Be(new DateTime(1000, 1, 1, 0, 16, 0));
            times[2].End.Should().Be(new DateTime(1000, 1, 1, 0, 21, 0));
        }

        [Test]
        public void GroupTimesOffsets_StartIsEnd_Test()
        {
            _serie3.EndTime = new DateTime(1000, 1, 1, 0, 18, 0);
            _serie3.NextRow = null;
            var times = _serie3.GroupTimesByTrigger(c => c.TriggerWhen(i => i.Value > 5).StartIsEnd()
                );

            times.Count.Should().Be(3);
            times[0].Start.Should().Be(new DateTime(1000, 1, 1, 0, 1, 0));
            times[0].End.Should().Be(new DateTime(1000, 1, 1, 0, 1, 0));
            times[1].Start.Should().Be(new DateTime(1000, 1, 1, 0, 14, 0));
            times[1].End.Should().Be(new DateTime(1000, 1, 1, 0, 14, 0));
            times[2].Start.Should().Be(new DateTime(1000, 1, 1, 0, 18, 0));
            times[2].End.Should().Be(new DateTime(1000, 1, 1, 0, 18, 0));
        }


    }
}