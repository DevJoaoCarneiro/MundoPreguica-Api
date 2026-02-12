using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Common
{
    public abstract class Entity
    {
        private readonly List<IDomainEvent> _domainEvent = new();

        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvent.AsReadOnly();

        protected void AddDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvent.Add(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvent.Clear();
        }
    }
}
