using System;
using System.Threading;
using UnityEngine;

namespace UnityUtils {
    public static class AwaitableExtensions {
        /// <summary>
        /// Awaits for the specified number of rendered frames.
        /// </summary>
        /// <param name="frameCount">Number of frames to wait. Must be zero or greater.</param>
        /// <param name="cancellationToken">Optional token used to cancel while waiting.</param>
        /// <returns>An awaitable that completes after the requested number of frames.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="frameCount"/> is negative.</exception>
        /// <example>
        /// <code>
        /// // Wait two frames before enabling input.
        /// await 2.DelayFramesAsync();
        /// inputEnabled = true;
        /// </code>
        /// </example>
        public static async Awaitable DelayFramesAsync(this int frameCount, CancellationToken cancellationToken = default) {
            if (frameCount < 0) throw new ArgumentOutOfRangeException(nameof(frameCount));

            for (var i = 0; i < frameCount; i++) {
                cancellationToken.ThrowIfCancellationRequested();
                await Awaitable.NextFrameAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Runs synchronous work on a background thread, then returns to the main thread with the result.
        /// </summary>
        /// <typeparam name="T">The return type of the work delegate.</typeparam>
        /// <param name="work">Synchronous work to execute on a background thread.</param>
        /// <param name="cancellationToken">Optional token used to cancel before or after the work runs.</param>
        /// <returns>An awaitable that yields the work result on the main thread.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="work"/> is null.</exception>
        /// <example>
        /// <code>
        /// Func&lt;int&gt; work = CalculateExpensiveValue;
        /// int expensiveValue = await work.RunOnBackgroundThread();
        /// hudLabel.text = expensiveValue.ToString();
        /// </code>
        /// </example>
        public static async Awaitable<T> RunOnBackgroundThread<T>(this Func<T> work, CancellationToken cancellationToken = default) {
            if (work == null) throw new ArgumentNullException(nameof(work));

            await Awaitable.BackgroundThreadAsync();
            cancellationToken.ThrowIfCancellationRequested();

            var result = work();

            await Awaitable.MainThreadAsync();
            cancellationToken.ThrowIfCancellationRequested();

            return result;
        }

        /// <summary>
        /// Repeatedly polls a condition until it returns true.
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="pollIntervalMs">Time between condition checks in milliseconds. Default is 33ms (≈ one frame at 30 FPS).</param>
        /// <returns>An Awaitable that completes when the condition returns true.</returns>
        /// <exception cref="ArgumentNullException">Thrown when condition is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when pollIntervalMs is not positive.</exception>
        public static Awaitable WaitUntil(this Func<bool> condition, int pollIntervalMs = 33) {
            if (condition == null) throw new ArgumentNullException(nameof(condition));
            if (pollIntervalMs <= 0)
                throw new ArgumentOutOfRangeException(nameof(pollIntervalMs), "Poll interval must be positive");

            var source = new AwaitableCompletionSource();

            if (condition()) {
                source.SetResult();
                return source.Awaitable;
            }

            var interval = TimeSpan.FromMilliseconds(pollIntervalMs);

            async void Poll() {
                while (!condition()) {
                    await Awaitable.WaitForSecondsAsync((float)interval.TotalSeconds);
                }

                source.SetResult();
            }

            Poll();
            return source.Awaitable;
        }
    }
}