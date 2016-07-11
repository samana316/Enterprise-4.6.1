using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Linq.Reactive
{
    partial class AsyncObservable
    {
        public static IAsyncObservable<TResult> GroupJoin
            <TLeft, TRight, TLeftDuration, TRightDuration, TResult>(
                this IAsyncObservable<TLeft> left,
                IAsyncObservable<TRight> right, 
                Func<TLeft, IAsyncObservable<TLeftDuration>> leftDurationSelector, 
                Func<TRight, IAsyncObservable<TRightDuration>> rightDurationSelector, 
                Func<TLeft, IAsyncObservable<TRight>, TResult> resultSelector)
        {
            Check.NotNull(left, "left");
            Check.NotNull(right, "right");
            Check.NotNull(leftDurationSelector, "leftDurationSelector");
            Check.NotNull(rightDurationSelector, "rightDurationSelector");
            Check.NotNull(resultSelector, "resultSelector");

            return new GroupJoinAsyncObservable<TLeft, TRight, TLeftDuration, TRightDuration, TResult>(
                left, right, leftDurationSelector, rightDurationSelector, resultSelector);
        }

        public static IAsyncObservable<TResult> Join<TLeft, TRight, TLeftDuration, TRightDuration, TResult>(
            this IAsyncObservable<TLeft> left,
            IAsyncObservable<TRight> right, 
            Func<TLeft, IAsyncObservable<TLeftDuration>> leftDurationSelector, 
            Func<TRight, IAsyncObservable<TRightDuration>> rightDurationSelector, 
            Func<TLeft, TRight, TResult> resultSelector)
        {
            Check.NotNull(left, "left");
            Check.NotNull(right, "right");
            Check.NotNull(leftDurationSelector, "leftDurationSelector");
            Check.NotNull(rightDurationSelector, "rightDurationSelector");
            Check.NotNull(resultSelector, "resultSelector");

            return new JoinAsyncObservable<TLeft, TRight, TLeftDuration, TRightDuration, TResult>(
                left, right, leftDurationSelector, rightDurationSelector, resultSelector);
        }

        private sealed class GroupJoinAsyncObservable<TLeft, TRight, TLeftDuration, TRightDuration, TResult>
            : AsyncObservableBase<TResult>
        {
            private readonly IAsyncObservable<TLeft> left;

            private readonly IAsyncObservable<TRight> right;

            private readonly Func<TLeft, IAsyncObservable<TLeftDuration>> leftDurationSelector;

            private readonly Func<TRight, IAsyncObservable<TRightDuration>> rightDurationSelector;

            private readonly Func<TLeft, IAsyncObservable<TRight>, TResult> resultSelector;

            public GroupJoinAsyncObservable(
                IAsyncObservable<TLeft> left, 
                IAsyncObservable<TRight> right, 
                Func<TLeft, IAsyncObservable<TLeftDuration>> leftDurationSelector, 
                Func<TRight, IAsyncObservable<TRightDuration>> rightDurationSelector, 
                Func<TLeft, IAsyncObservable<TRight>, TResult> resultSelector)
            {
                this.left = left;
                this.right = right;
                this.leftDurationSelector = leftDurationSelector;
                this.rightDurationSelector = rightDurationSelector;
                this.resultSelector = resultSelector;
            }

            protected override Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<TResult> observer, 
                CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        private sealed class JoinAsyncObservable<TLeft, TRight, TLeftDuration, TRightDuration, TResult> 
            : AsyncObservableBase<TResult>
        {
            private readonly IAsyncObservable<TLeft> left;

            private readonly Func<TLeft, IAsyncObservable<TLeftDuration>> leftDurationSelector;

            private readonly Func<TLeft, TRight, TResult> resultSelector;

            private readonly IAsyncObservable<TRight> right;

            private readonly Func<TRight, IAsyncObservable<TRightDuration>> rightDurationSelector;

            public JoinAsyncObservable(
                IAsyncObservable<TLeft> left, 
                IAsyncObservable<TRight> right, 
                Func<TLeft, IAsyncObservable<TLeftDuration>> leftDurationSelector, 
                Func<TRight, IAsyncObservable<TRightDuration>> 
                rightDurationSelector, Func<TLeft, TRight, TResult> resultSelector)
            {
                this.left = left;
                this.right = right;
                this.leftDurationSelector = leftDurationSelector;
                this.rightDurationSelector = rightDurationSelector;
                this.resultSelector = resultSelector;
            }

            protected override Task<IDisposable> SubscribeCoreAsync(
                IAsyncObserver<TResult> observer, 
                CancellationToken cancellationToken)
            {
                var joinImpl = new JoinAsyncImpl(this, observer);

                return joinImpl.RunAsync(cancellationToken);
            }

            private sealed class JoinAsyncImpl
            {
                private readonly JoinAsyncObservable<TLeft, TRight, TLeftDuration, TRightDuration, TResult> parent;

                private readonly IAsyncObserver<TResult> observer;

                private bool leftDone;

                private int leftID;

                private IDictionary<int, TLeft> leftMap;

                private bool rightDone;

                private int rightID;

                private IDictionary<int, TRight> rightMap;

                public JoinAsyncImpl(
                    JoinAsyncObservable<TLeft, TRight, TLeftDuration, TRightDuration, TResult> parent, 
                    IAsyncObserver<TResult> observer)
                {
                    this.parent = parent;
                    this.observer = observer;
                }

                public async Task<IDisposable> RunAsync(
                    CancellationToken cancellationToken)
                {
                    this.leftMap = new Dictionary<int, TLeft>();
                    this.rightMap = new Dictionary<int, TRight>();

                    await this.parent.left.SubscribeRawAsync(new LeftAsyncObserver(this));
                    await this.parent.right.SubscribeSafeAsync(new RightAsyncObserver(this));

                    return null;
                }

                private sealed class LeftAsyncObserver : AsyncSink<TLeft>
                {
                    private readonly JoinAsyncImpl parent;

                    public LeftAsyncObserver(
                        JoinAsyncImpl parent)
                        : base(parent.observer.AsPartial())
                    {
                        this.parent = parent;
                    }

                    protected override async Task OnNextCoreAsync(
                        TLeft value, 
                        CancellationToken cancellationToken)
                    {
                        var id = this.parent.leftID++;
                        this.parent.leftMap.Add(id, value);

                        var observable = this.parent.parent.leftDurationSelector(value);
                        await observable.ForEachAsync(x => this.Expire(id));

                        foreach (var current in this.parent.rightMap.Values)
                        {
                            var result = this.parent.parent.resultSelector(value, current);

                            await this.parent.observer.OnNextAsync(result, cancellationToken);
                        }
                    }

                    private void Expire(
                       int id)
                    {
                        if (this.parent.leftMap.Remove(id) &&
                            this.parent.leftMap.Count == 0 &&
                            this.parent.leftDone)
                        {
                            this.parent.observer.OnCompleted();
                        }
                    }
                }

                private sealed class RightAsyncObserver : AsyncSink<TRight>
                {
                    private readonly JoinAsyncImpl parent;

                    public RightAsyncObserver(
                        JoinAsyncImpl parent)
                        : base(parent.observer.AsPartial())
                    {
                        this.parent = parent;
                    }

                    protected override async Task OnNextCoreAsync(
                        TRight value, 
                        CancellationToken cancellationToken)
                    {
                        var id = this.parent.rightID++;
                        this.parent.rightMap.Add(id, value);

                        var observable = this.parent.parent.rightDurationSelector(value);
                        await observable.ForEachAsync(x => this.Expire(id));

                        foreach (var current in this.parent.leftMap.Values)
                        {
                            var result = this.parent.parent.resultSelector(current, value);

                            await this.parent.observer.OnNextAsync(result, cancellationToken);
                        }
                    }

                    private void Expire(
                        int id)
                    {
                        if (this.parent.rightMap.Remove(id) &&
                            this.parent.rightMap.Count == 0 &&
                            this.parent.rightDone)
                        {
                            this.parent.observer.OnCompleted();
                        }
                    }
                }
            }
        }
    }
}
