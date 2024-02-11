using System;
using System.Collections.Generic;
using System.Reactive.Disposables;

namespace System.Reactive.Linq.ObservableImpl
{
    internal class Join<TLeft, TRight, TLeftDuration, TRightDuration, TResult> : IObservable<TResult>
    {
        private readonly IObservable<TLeft> _left;

        private readonly IObservable<TRight> _right;

        private readonly Func<TLeft, IObservable<TLeftDuration>> _leftDurationSelector;

        private readonly Func<TRight, IObservable<TRightDuration>> _rightDurationSelector;

        private readonly Func<TLeft, TRight, TResult> _resultSelector;

        public Join(IObservable<TLeft> left, IObservable<TRight> right, Func<TLeft, IObservable<TLeftDuration>> leftDurationSelector, Func<TRight, IObservable<TRightDuration>> rightDurationSelector, Func<TLeft, TRight, TResult> resultSelector)
        {
            this._left = left;
            this._right = right;
            this._leftDurationSelector = leftDurationSelector;
            this._rightDurationSelector = rightDurationSelector;
            this._resultSelector = resultSelector;
        }

        protected override IDisposable Run(IObserver<TResult> observer, IDisposable cancel, Action<IDisposable> setSink)
        {
            Join<TLeft, TRight, TLeftDuration, TRightDuration, TResult>._ _ = new Join<TLeft, TRight, TLeftDuration, TRightDuration, TResult>._(this, observer, cancel);
            setSink(_);
            return _.Run();
        }

        private class _ : Sink<TResult>
        {
            private readonly Join<TLeft, TRight, TLeftDuration, TRightDuration, TResult> _parent;

            private object _gate;

            private CompositeDisposable _group;

            private bool _leftDone;

            private int _leftID;

            private Dictionary<int, TLeft> _leftMap;

            private bool _rightDone;

            private int _rightID;

            private Dictionary<int, TRight> _rightMap;

            public _(Join<TLeft, TRight, TLeftDuration, TRightDuration, TResult> parent, IObserver<TResult> observer, IDisposable cancel) : base(observer, cancel)
            {
                this._parent = parent;
            }

            public IDisposable Run()
            {
                this._gate = new object();
                this._group = new CompositeDisposable();
                SingleAssignmentDisposable singleAssignmentDisposable = new SingleAssignmentDisposable();
                this._group.Add(singleAssignmentDisposable);
                this._leftDone = false;
                this._leftID = 0;
                this._leftMap = new Dictionary<int, TLeft>();
                SingleAssignmentDisposable singleAssignmentDisposable2 = new SingleAssignmentDisposable();
                this._group.Add(singleAssignmentDisposable2);
                this._rightDone = false;
                this._rightID = 0;
                this._rightMap = new Dictionary<int, TRight>();
                singleAssignmentDisposable.set_Disposable(ObservableExtensions.SubscribeSafe<TLeft>(this._parent._left, new Join<TLeft, TRight, TLeftDuration, TRightDuration, TResult>._.LeftObserver(this, singleAssignmentDisposable)));
                singleAssignmentDisposable2.set_Disposable(ObservableExtensions.SubscribeSafe<TRight>(this._parent._right, new Join<TLeft, TRight, TLeftDuration, TRightDuration, TResult>._.RightObserver(this, singleAssignmentDisposable2)));
                return this._group;
            }

            private class LeftObserver : IObserver<TLeft>
            {
                private readonly Join<TLeft, TRight, TLeftDuration, TRightDuration, TResult>._ _parent;

                private readonly IDisposable _self;

                public LeftObserver(Join<TLeft, TRight, TLeftDuration, TRightDuration, TResult>._ parent, IDisposable self)
                {
                    this._parent = parent;
                    this._self = self;
                }

                private void Expire(int id, IDisposable resource)
                {
                    lock (this._parent._gate)
                    {
                        if (this._parent._leftMap.Remove(id) && this._parent._leftMap.Count == 0 && this._parent._leftDone)
                        {
                            this._parent._observer.OnCompleted();
                            this._parent.Dispose();
                        }
                    }
                    this._parent._group.Remove(resource);
                }

                public void OnNext(TLeft value)
                {
                    int num = 0;
                    lock (this._parent._gate)
                    {
                        num = this._parent._leftID++;
                        this._parent._leftMap.Add(num, value);
                    }
                    SingleAssignmentDisposable singleAssignmentDisposable = new SingleAssignmentDisposable();
                    this._parent._group.Add(singleAssignmentDisposable);
                    IObservable<TLeftDuration> observable = null;
                    try
                    {
                        observable = this._parent._parent._leftDurationSelector(value);
                    }
                    catch (Exception error)
                    {
                        this._parent._observer.OnError(error);
                        this._parent.Dispose();
                        return;
                    }
                    singleAssignmentDisposable.set_Disposable(ObservableExtensions.SubscribeSafe<TLeftDuration>(observable, new Join<TLeft, TRight, TLeftDuration, TRightDuration, TResult>._.LeftObserver.Delta(this, num, singleAssignmentDisposable)));
                    lock (this._parent._gate)
                    {
                        foreach (TRight current in this._parent._rightMap.Values)
                        {
                            TResult value2 = default(TResult);
                            try
                            {
                                value2 = this._parent._parent._resultSelector(value, current);
                            }
                            catch (Exception error2)
                            {
                                this._parent._observer.OnError(error2);
                                this._parent.Dispose();
                                break;
                            }
                            this._parent._observer.OnNext(value2);
                        }
                    }
                }

                public void OnError(Exception error)
                {
                    lock (this._parent._gate)
                    {
                        this._parent._observer.OnError(error);
                        this._parent.Dispose();
                    }
                }

                public void OnCompleted()
                {
                    lock (this._parent._gate)
                    {
                        this._parent._leftDone = true;
                        if (this._parent._rightDone || this._parent._leftMap.Count == 0)
                        {
                            this._parent._observer.OnCompleted();
                            this._parent.Dispose();
                        }
                        else
                        {
                            this._self.Dispose();
                        }
                    }
                }

                private class Delta : IObserver<TLeftDuration>
                {
                    private readonly Join<TLeft, TRight, TLeftDuration, TRightDuration, TResult>._.LeftObserver _parent;

                    private readonly int _id;

                    private readonly IDisposable _self;

                    public Delta(Join<TLeft, TRight, TLeftDuration, TRightDuration, TResult>._.LeftObserver parent, int id, IDisposable self)
                    {
                        this._parent = parent;
                        this._id = id;
                        this._self = self;
                    }

                    public void OnNext(TLeftDuration value)
                    {
                        this._parent.Expire(this._id, this._self);
                    }

                    public void OnError(Exception error)
                    {
                        this._parent.OnError(error);
                    }

                    public void OnCompleted()
                    {
                        this._parent.Expire(this._id, this._self);
                    }
                }
            }

            private class RightObserver : IObserver<TRight>
            {
                private class Delta : IObserver<TRightDuration>
                {
                    private readonly Join<TLeft, TRight, TLeftDuration, TRightDuration, TResult>._.RightObserver _parent;

                    private readonly int _id;

                    private readonly IDisposable _self;

                    public Delta(Join<TLeft, TRight, TLeftDuration, TRightDuration, TResult>._.RightObserver parent, int id, IDisposable self)
                    {
                        this._parent = parent;
                        this._id = id;
                        this._self = self;
                    }

                    public void OnNext(TRightDuration value)
                    {
                        this._parent.Expire(this._id, this._self);
                    }

                    public void OnError(Exception error)
                    {
                        this._parent.OnError(error);
                    }

                    public void OnCompleted()
                    {
                        this._parent.Expire(this._id, this._self);
                    }
                }

                private readonly Join<TLeft, TRight, TLeftDuration, TRightDuration, TResult>._ _parent;

                private readonly IDisposable _self;

                public RightObserver(Join<TLeft, TRight, TLeftDuration, TRightDuration, TResult>._ parent, IDisposable self)
                {
                    this._parent = parent;
                    this._self = self;
                }

                private void Expire(int id, IDisposable resource)
                {
                    lock (this._parent._gate)
                    {
                        if (this._parent._rightMap.Remove(id) && this._parent._rightMap.Count == 0 && this._parent._rightDone)
                        {
                            this._parent._observer.OnCompleted();
                            this._parent.Dispose();
                        }
                    }
                    this._parent._group.Remove(resource);
                }

                public void OnNext(TRight value)
                {
                    int num = 0;
                    lock (this._parent._gate)
                    {
                        num = this._parent._rightID++;
                        this._parent._rightMap.Add(num, value);
                    }
                    SingleAssignmentDisposable singleAssignmentDisposable = new SingleAssignmentDisposable();
                    this._parent._group.Add(singleAssignmentDisposable);
                    IObservable<TRightDuration> observable = null;
                    try
                    {
                        observable = this._parent._parent._rightDurationSelector(value);
                    }
                    catch (Exception error)
                    {
                        this._parent._observer.OnError(error);
                        this._parent.Dispose();
                        return;
                    }
                    singleAssignmentDisposable.set_Disposable(ObservableExtensions.SubscribeSafe<TRightDuration>(observable, new Join<TLeft, TRight, TLeftDuration, TRightDuration, TResult>._.RightObserver.Delta(this, num, singleAssignmentDisposable)));
                    lock (this._parent._gate)
                    {
                        foreach (TLeft current in this._parent._leftMap.Values)
                        {
                            TResult value2 = default(TResult);
                            try
                            {
                                value2 = this._parent._parent._resultSelector(current, value);
                            }
                            catch (Exception error2)
                            {
                                this._parent._observer.OnError(error2);
                                this._parent.Dispose();
                                break;
                            }
                            this._parent._observer.OnNext(value2);
                        }
                    }
                }

                public void OnError(Exception error)
                {
                    lock (this._parent._gate)
                    {
                        this._parent._observer.OnError(error);
                        this._parent.Dispose();
                    }
                }

                public void OnCompleted()
                {
                    lock (this._parent._gate)
                    {
                        this._parent._rightDone = true;
                        if (this._parent._leftDone || this._parent._rightMap.Count == 0)
                        {
                            this._parent._observer.OnCompleted();
                            this._parent.Dispose();
                        }
                        else
                        {
                            this._self.Dispose();
                        }
                    }
                }
            }
        }
    }
}
