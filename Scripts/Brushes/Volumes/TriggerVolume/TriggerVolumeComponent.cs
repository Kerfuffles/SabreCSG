using UnityEngine;

namespace Sabresaurus.SabreCSG.Volumes
{
	/// <summary>
	/// Executes trigger logic when objects interact with the volume.
	/// </summary>
	/// <seealso cref="UnityEngine.MonoBehaviour" />
	public class TriggerVolumeComponent : MonoBehaviour
	{
		/// <summary>
		/// The trigger type, this is reserved for future use.
		/// </summary>
		public TriggerVolumeTriggerType triggerType = TriggerVolumeTriggerType.UnityEvent;

		/// <summary>
		/// Whether to use a filter tag.
		/// </summary>
		public bool useFilterTag = false;

		/// <summary>
		/// The filter tag to limit the colliders that can invoke the trigger.
		/// </summary>
		public string filterTag = "Untagged";

		/// <summary>
		/// The layer mask to limit the colliders that can invoke the trigger.
		/// </summary>
		public LayerMask layer = -1;

		/// <summary>
		/// Whether the trigger can only be instigated once.
		/// </summary>
		public bool triggerOnceOnly = false;

		/// <summary>
		/// Do we require a "use" event for this trigger? Only relevant to <see cref="onStayEvent"/>.
		/// </summary>
		public bool requiresUse = false;

		/// <summary>
		/// If we are using the input manager, then which input event do we use?
		/// </summary>
		public string inputEventName = "Submit";

		/// <summary>
		/// If we are using <see cref="KeyCode"/>, then which key do we use?
		/// </summary>
		public KeyCode inputKey = KeyCode.E;

		/// <summary>
		/// Which input type do we use?
		/// </summary>
		[SerializeField]
		public UseInputType useInputType = UseInputType.KeyCode;

		/// <summary>
		/// The event called when a collider enters the trigger volume.
		/// </summary>
		public TriggerVolumeEvent onEnterEvent;

		/// <summary>
		/// The event called when a collider stays in the trigger volume.
		/// </summary>
		public TriggerVolumeEvent onStayEvent;

		/// <summary>
		/// The event called when a collider exits the trigger volume.
		/// </summary>
		public TriggerVolumeEvent onExitEvent;

		/// <summary>
		/// Whether the trigger can still be triggered (used with <see cref="triggerOnceOnly"/>).
		/// </summary>
		private bool canTrigger = true;

		/// <summary>
		/// Called when a collider enters the volume.
		/// </summary>
		/// <param name="other">The collider that entered the volume.</param>
		private void OnTriggerEnter( Collider other )
		{
			// require input:
			if( requiresUse )
				return;

			// ignore empty events.
			if( onEnterEvent.GetPersistentEventCount() == 0 )
				return;
			// tag filter:
			if( useFilterTag && other.tag != filterTag )
				return;
			// layer filter:
			if( !layer.Contains( other.gameObject.layer ) )
				return;
			// trigger once only:
			if( !triggerOnceOnly )
				canTrigger = true;
			if( !canTrigger )
				return;
			if( triggerOnceOnly )
				canTrigger = false;

			switch( triggerType )
			{
				case TriggerVolumeTriggerType.UnityEvent:
					onEnterEvent.Invoke();
					break;
			}
		}

		/// <summary>
		/// Called when a collider exits the volume.
		/// </summary>
		/// <param name="other">The collider that exited the volume.</param>
		private void OnTriggerExit( Collider other )
		{
			// require input
			if( requiresUse )
				return;

			// ignore empty events.
			if( onExitEvent.GetPersistentEventCount() == 0 )
				return;
			// tag filter:
			if( useFilterTag && other.tag != filterTag )
				return;
			// layer filter:
			if( !layer.Contains( other.gameObject.layer ) )
				return;
			// trigger once only:
			if( !triggerOnceOnly )
				canTrigger = true;
			if( !canTrigger )
				return;
			if( triggerOnceOnly )
				canTrigger = false;

			switch( triggerType )
			{
				case TriggerVolumeTriggerType.UnityEvent:
					onExitEvent.Invoke();
					break;
			}
		}

		/// <summary>
		/// Called every frame while a collider stays inside the volume.
		/// </summary>
		/// <param name="other">The collider that is inside of the volume.</param>
		private void OnTriggerStay( Collider other )
		{
			// ignore empty events.
			if( onStayEvent.GetPersistentEventCount() == 0 )
				return;
			// tag filter:
			if( useFilterTag && other.tag != filterTag )
				return;
			// layer filter:
			if( !layer.Contains( other.gameObject.layer ) )
				return;
			// trigger once only:
			if( !triggerOnceOnly )
				canTrigger = true;
			if( !canTrigger )
				return;

			switch( triggerType )
			{
				case TriggerVolumeTriggerType.UnityEvent:

					if( requiresUse )
					{
						switch( useInputType )
						{
							case UseInputType.InputManager:
								if( Input.GetButtonDown( inputEventName ) && triggerOnceOnly )
								{
									onStayEvent.Invoke();
									canTrigger = false;
								}
								else if( Input.GetButtonDown( inputEventName ) )
									onStayEvent.Invoke();

								break;

							case UseInputType.KeyCode:
								if( Input.GetKeyDown( inputKey ) && triggerOnceOnly )
								{
									onStayEvent.Invoke();
									canTrigger = false;
								}
								else if( Input.GetKeyDown( inputKey ) )
									onStayEvent.Invoke();

								break;
						}
					}
					else
						onStayEvent.Invoke();

					break;
			}
		}
	}
}
