using System;
using MemoQ.MTInterfaces;

namespace MT_SDK
{
    /// <summary>
    /// Describes an MT engine that is currently active.
    /// </summary>
    /// <remarks>
    /// Abstracts two distinct interfaces plugins can implement; common handling by two distinct implementation of this interface.
    /// </remarks>
    internal interface ICurrentEngine : IDisposable
    {
        ISession CreateLookupSession();
        ISessionForStoringTranslations CreateSessionForStoringTranslation();
    }

    /// <summary>
    /// Describes a plugin engine that implements <see cref="IEngine"/> interface.
    /// </summary>
    internal class CurrentEngine : ICurrentEngine
    {
        private readonly IEngine engine;
        public CurrentEngine(IEngine engine) { this.engine = engine; }

        public ISession CreateLookupSession() => engine.CreateSession();
        public void Dispose() => engine?.Dispose();
        public ISessionForStoringTranslations CreateSessionForStoringTranslation()
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Describes a plugin engine that implements <see cref="IEngine2"/> interface.
    /// </summary>
    internal class CurrentEngine2 : ICurrentEngine
    {
        private readonly IEngine2 engine;
        public CurrentEngine2(IEngine2 engine) { this.engine = engine; }

        public ISession CreateLookupSession() => engine.CreateLookupSession();
        public ISessionForStoringTranslations CreateSessionForStoringTranslation() => engine.CreateStoreTranslationSession();
        public void Dispose() => engine?.Dispose();
    }
}
