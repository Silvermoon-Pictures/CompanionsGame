using System;
using System.Collections.Generic;
using Companions.Common;
using Silvermoon.Core;
using UnityEngine;

namespace Companions.Systems
{
    [RequiredSystem]
    public class NotificationSystem : BaseSystem<NotificationSystem>
    {
        private List<Notification> notifications = new();
        
        protected override void Initialize(GameContext context)
        {
            base.Initialize(context);

            Notify<ICompanionComponent>(NotifyInitialized);
        }

        protected override void Cleanup()
        {
            base.Cleanup();
            
            foreach (var notification in notifications)
                notification.Cleanup();
            
            notifications.Clear();
        }

        public static void Notify<TNotifiedType>(Action<TNotifiedType> action) where TNotifiedType : ICoreComponent
        {
            Instance.notifications.Add(new Notification<TNotifiedType>(action));
        }
        
        private void NotifyInitialized(ICompanionComponent component) 
            => component.Initialize(GameContext);
    }
}

public abstract class Notification
{
    internal abstract void Cleanup();
}

public class Notification<TNotifiedType> : Notification where TNotifiedType : ICoreComponent
{
    ComponentGroup ComponentGroup { get; set; }
    private Action<TNotifiedType> Action { get; set; }
    
    public Notification(Action<TNotifiedType> action)
    {
        ComponentGroup = ComponentSystem.GetComponentGroup<TNotifiedType>();
        Action = action;

        foreach (var component in ComponentGroup.Components)
            action((TNotifiedType)component);
        
        ComponentGroup.OnComponentAdded += OnComponentAdded;
    }

    private void OnComponentAdded(object sender, ICoreComponent e)
    {
        Action?.Invoke((TNotifiedType)e);
    }

    internal override void Cleanup()
    {
        Action = null;
        ComponentGroup = null;
    }
}