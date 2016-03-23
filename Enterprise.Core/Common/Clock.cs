using System;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Common
{
    public abstract class Clock
    {
        private static readonly Clock local = new LocalClock();

        private static readonly Clock utc = new UtcClock();

        public static Clock Local
        {
            get { return local; }
        }

        public static Clock Utc
        {
            get { return utc; }
        }

        public abstract DateTime CurrentDateTime { get; }

        public static Clock Create(
            Func<DateTime> function)
        {
            Check.NotNull(function, "function");

            return new AnonymouseClock(function);
        }

        public static Clock CreateFake(
            DateTime value)
        {
            return new FakeClock(value);
        }

        private class LocalClock : Clock
        {
            public override DateTime CurrentDateTime
            {
                get { return DateTime.Now; }
            }
        }

        private class UtcClock : Clock
        {
            public override DateTime CurrentDateTime
            {
                get { return DateTime.UtcNow; }
            }
        }

        private class AnonymouseClock : Clock
        {
            private readonly Func<DateTime> function;

            public AnonymouseClock(
                Func<DateTime> function)
            {
                this.function = function;
            }

            public override DateTime CurrentDateTime
            {
                get
                {
                    return function();
                }
            }
        }

        private class FakeClock : Clock
        {
            private readonly DateTime value;

            public FakeClock(
                DateTime value)
            {
                this.value = value;
            }

            public override DateTime CurrentDateTime
            {
                get { return this.value; }
            }
        }
    }
}
