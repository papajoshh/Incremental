using System;

namespace Runtime.Application
{
    public class SkippableAction : ISkippable
    {
        private readonly Action _action;

        public SkippableAction(Action action)
        {
            _action = action;
        }

        public void Skip() => _action();
    }
}
