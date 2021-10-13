using System;
using UnityEngine;

namespace nfg.Unity.Utils {

    public class Debouncer : Debouncer<bool> {

        // Use our own internal pendingAction so we don't need to implement the Generic
        private Action pendingAction;

        /// <summary>
        /// Debouncer for Unity Lifecycle components, for an Action without any input.
        ///
        /// NOTE: This must be initialized after UnityEngine has started up, for `Time` to be accessible.
        ///       Simply initialize during OnEnable() to ensure accessiblity
        ///       (see: https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnEnable.html)
        /// </summary>
        public Debouncer(float debounceRate = 0.1f) : base(debounceRate) { }

        /// <summary>
        /// Debounce desired Function, with input.
        /// This will automatically kick off a `DebounceCheck` as well, but be sure to utilize
        /// `DebounceCheck` in an Update cycle of some sort to reconcile the last call to this method.
        /// </summary>
        public void Debounce(Action function) {
            pendingAction = function;
            DebounceCheck();
        }

        /// <summary>
        /// Check if Elapsed time exceeds the Debounce Rate, and Execute if necessary.
        /// Automatically called upon Debouncing, but should be called during  
        /// </summary>
        public override void DebounceCheck() {
            if (null == pendingAction) {
                return;
            }

            if (HasElapsed) {
                pendingAction.Invoke();
                lastExec = Time.time;
                pendingAction = null;
            }
        }
    }

    public class Debouncer<T> {
        // How long Elapsed time must exceed to Execute
        protected float debounceRate = 0.1f;
        // Last time the Debounce successfully Executed the Pending Action
        protected float lastExec;

        // The Action we want to run once the Duration has Elapsed
        private Action<T> pendingAction;
        // The Data we want to run inject into the Action once the Duration has Elapsed
        private T pendingData;

        /// <summary>
        ///   Has the Debounce Rate Elapsed?
        ///   If the Debounce hasn't elapsed, and there is a Pending action, we are Pending!
        /// </summary>
        public bool IsPending {
            get => !HasElapsed || null != pendingAction;
        }

        /// <summary>
        ///   Gets the Elapsed time since last Executed
        /// </summary>
        protected float ElapsedTime {
            get => Time.time - lastExec;
        }

        /// <summary>
        ///   Has the Debounce Rate Elapsed?
        /// </summary>
        protected bool HasElapsed {
            get => ElapsedTime > debounceRate;
        }

        /// <summary>
        /// Debouncer for Unity Lifecycle components, for an Action that takes an input parameter of type T.
        ///
        /// NOTE: This must be initialized after UnityEngine has started up, for `Time` to be accessible.
        ///       Simply initialize during OnEnable() to ensure accessiblity
        ///       (see: https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnEnable.html)
        /// </summary>
        public Debouncer(float debounceRate = 0.1f) {
            this.debounceRate = debounceRate;
            this.lastExec = Time.time;
        }

        /// <summary>
        /// Debounce desired Function, with input.
        /// This will automatically kick off a `DebounceCheck` as well, but be sure to utilize
        /// `DebounceCheck` in an Update cycle of some sort to reconcile the last call to this method.
        /// </summary>
        public void Debounce(Action<T> debounceFunction, T input) {
            pendingAction = debounceFunction;
            pendingData = input;
            DebounceCheck();
        }

        /// <summary>
        /// Check if Elapsed time exceeds the Debounce Rate, and Execute if necessary.
        /// Automatically called upon Debouncing, but should be called during  
        /// </summary>
        public virtual void DebounceCheck() {
            if (null == pendingAction) {
                return;
            }

            if (HasElapsed) {
                pendingAction.Invoke(pendingData);
                lastExec = Time.time;
                pendingAction = null;
            }
        }
    }
}