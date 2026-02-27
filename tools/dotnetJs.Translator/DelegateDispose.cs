namespace dotnetJs.Translator
{
    class DelegateDispose : IDisposable
        {
            Action dispose;

            public DelegateDispose(Action dispose)
            {
                this.dispose = dispose;
            }

            public void Dispose()
            {
                dispose();
            }
        }
}
