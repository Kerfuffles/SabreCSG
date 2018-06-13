#if UNITY_EDITOR || RUNTIME_CSG

using System;
using System.Linq;
using UnityEngine;

namespace Sabresaurus.SabreCSG.Volumes
{
	/// <summary>
	/// Executes trigger logic when objects interact with the volume.
	/// </summary>
	/// <seealso cref="Sabresaurus.SabreCSG.Volume"/>
	[Serializable]
	public class TriggerVolume : Volume
	{
		/// <summary>
		/// Gets the brush preview material shown in the editor.
		/// </summary>
		public override Material BrushPreviewMaterial
		{
			get
			{
				return (Material)SabreCSGResources.LoadObject( "Resources/Materials/scsg_volume_trigger.mat" );
			}
		}

		/// <summary>
		/// The trigger type, this is reserved for future use.
		/// </summary>
		[SerializeField]
		public TriggerVolumeTriggerType triggerType = TriggerVolumeTriggerType.UnityEvent;

		/// <summary>
		/// Whether to use a filter tag.
		/// </summary>
		[SerializeField]
		public bool useFilterTag = false;

		/// <summary>
		/// The filter tag to limit the colliders that can invoke the trigger.
		/// </summary>
		[SerializeField]
		public string filterTag = "Untagged";

		/// <summary>
		/// The layer mask to limit the colliders that can invoke the trigger.
		/// </summary>
		[SerializeField]
		public LayerMask layer = -1;

		/// <summary>
		/// Whether the trigger can only be instigated once.
		/// </summary>
		[SerializeField]
		public bool triggerOnceOnly = false;

		/// <summary>
		/// Do we require a "use" event for this trigger? Only relevant to <see cref="onStayEvent"/>.
		/// </summary>
		[SerializeField]
		public bool requiresUse = false;

		/// <summary>
		/// If we are using the input manager, then which input event do we use?
		/// </summary>
		[SerializeField]
		public string inputEventName = "Submit";

		/// <summary>
		/// If we are using <see cref="KeyCode"/>, then which key do we use?
		/// </summary>
		[SerializeField]
		public KeyCode inputKey = KeyCode.E;

		/// <summary>
		/// Which input type do we use?
		/// </summary>
		[SerializeField]
		public UseInputType useInputType = UseInputType.KeyCode;

		/// <summary>
		/// The event called when a collider enters the trigger volume.
		/// </summary>
		[SerializeField]
		public TriggerVolumeEvent onEnterEvent;

		/// <summary>
		/// The event called when a collider stays in the trigger volume.
		/// </summary>
		[SerializeField]
		public TriggerVolumeEvent onStayEvent;

		/// <summary>
		/// The event called when a collider exits the trigger volume.
		/// </summary>
		[SerializeField]
		public TriggerVolumeEvent onExitEvent;

#if UNITY_EDITOR

		/// <summary>
		/// Called when the inspector GUI is drawn in the editor.
		/// </summary>
		/// <param name="selectedVolumes">The selected volumes in the editor (for multi-editing).</param>
		/// <returns>True if a property changed or else false.</returns>
		public override bool OnInspectorGUI( Volume[] selectedVolumes )
		{
			var triggerVolumes = selectedVolumes.Cast<TriggerVolume>();
			bool invalidate = false;

			GUILayout.BeginVertical( "Box" );
			{
				UnityEditor.EditorGUILayout.LabelField( "Trigger Options", UnityEditor.EditorStyles.boldLabel );
				GUILayout.Space( 4 );

				UnityEditor.EditorGUI.indentLevel = 1;

				GUILayout.BeginVertical();
				{
					// this is hidden so that we can introduce more trigger types in the future.

					//TriggerVolumeTriggerType previousVolumeEventType;
					//triggerType = (TriggerVolumeTriggerType)UnityEditor.EditorGUILayout.EnumPopup(new GUIContent("Trigger Type"), previousVolumeEventType = triggerType);
					//if (triggerType != previousVolumeEventType)
					//{
					//    foreach (TriggerVolume volume in triggerVolumes)
					//        volume.triggerType = triggerType;
					//    invalidate = true;
					//}

					LayerMask previousLayerMask;
					layer = SabreGUILayout.LayerMaskField( new GUIContent( "Layer Mask", "The layer mask to limit the colliders that can invoke the trigger." ), ( previousLayerMask = layer ).value );
					if( previousLayerMask != layer )
					{
						foreach( TriggerVolume volume in triggerVolumes )
							volume.layer = layer;
						invalidate = true;
					}

					bool previousUseFilterTag;
					useFilterTag = UnityEditor.EditorGUILayout.Toggle( new GUIContent( "Use Filter Tag", "Whether to use a filter tag." ), previousUseFilterTag = useFilterTag );
					if( useFilterTag != previousUseFilterTag )
					{
						foreach( TriggerVolume volume in triggerVolumes )
							volume.useFilterTag = useFilterTag;
						invalidate = true;
					}

					if( useFilterTag )
					{
						string previousFilterTag;
						filterTag = UnityEditor.EditorGUILayout.TagField( new GUIContent( "Filter Tag", "The filter tag to limit the colliders that can invoke the trigger." ), previousFilterTag = filterTag );
						if( filterTag != previousFilterTag )
						{
							foreach( TriggerVolume volume in triggerVolumes )
								volume.filterTag = filterTag;
							invalidate = true;
						}
					}

					bool previousTriggerOnce;
					triggerOnceOnly = UnityEditor.EditorGUILayout.Toggle( new GUIContent( "Trigger Once Only", "Whether the trigger can only be instigated once." ), previousTriggerOnce = triggerOnceOnly );
					if( triggerOnceOnly != previousTriggerOnce )
					{
						foreach( TriggerVolume volume in triggerVolumes )
							volume.triggerOnceOnly = triggerOnceOnly;
						invalidate = true;
					}

					bool previousRequiresUse;
					requiresUse = UnityEditor.EditorGUILayout.Toggle( new GUIContent( "Require Use Event" ), previousRequiresUse = requiresUse );
					if( requiresUse != previousRequiresUse )
					{
						foreach( TriggerVolume volume in triggerVolumes )
							volume.requiresUse = requiresUse;
						invalidate = true;
					}
				}
				GUILayout.EndVertical();

				UnityEditor.EditorGUI.indentLevel = 0;
			}
			GUILayout.EndVertical();

			if( requiresUse )
			{
				GUILayout.BeginVertical( "Box" );
				{
					UnityEditor.EditorGUILayout.LabelField( "Use Settings", UnityEditor.EditorStyles.boldLabel );
					GUILayout.Space( 4 );

					UnityEditor.EditorGUI.indentLevel = 1;

					UseInputType oldUseInputType;
					useInputType = (UseInputType)UnityEditor.EditorGUILayout.EnumPopup( new GUIContent( "Input Type" ), oldUseInputType = useInputType );
					if( useInputType != oldUseInputType )
					{
						foreach( TriggerVolume volume in triggerVolumes )
							volume.useInputType = useInputType;
						invalidate = true;
					}

					switch( useInputType )
					{
						case UseInputType.InputManager:
							string oldInputEventName;
							inputEventName = UnityEditor.EditorGUILayout.TextField( new GUIContent( "Input Button", "The name of the input button set up in the input manager." ), oldInputEventName = inputEventName );
							if( inputEventName != oldInputEventName )
							{
								foreach( TriggerVolume volume in triggerVolumes )
									volume.inputEventName = inputEventName;
								invalidate = true;
							}

							break;

						case UseInputType.KeyCode:
							KeyCode oldInputKey;
							inputKey = (KeyCode)UnityEditor.EditorGUILayout.EnumPopup( new GUIContent( "Input Key" ), oldInputKey = inputKey );
							if( inputKey != oldInputKey )
							{
								foreach( TriggerVolume volume in triggerVolumes )
									volume.inputKey = inputKey;
								invalidate = true;
							}

							break;
					}

					UnityEditor.EditorGUI.indentLevel = 0;
				}
				GUILayout.EndVertical();
			}

			GUILayout.BeginVertical( "Box" );
			{
				UnityEditor.EditorGUILayout.LabelField( "Trigger Events", UnityEditor.EditorStyles.boldLabel );
				GUILayout.Space( 4 );

				if( triggerType == TriggerVolumeTriggerType.UnityEvent )
				{
					UnityEditor.EditorGUI.indentLevel = 1;

					GUILayout.BeginVertical();
					{
						UnityEditor.SerializedObject tv = new UnityEditor.SerializedObject( this );
						UnityEditor.SerializedProperty prop1 = tv.FindProperty( "onEnterEvent" );
						UnityEditor.SerializedProperty prop2 = tv.FindProperty( "onStayEvent" );
						UnityEditor.SerializedProperty prop3 = tv.FindProperty( "onExitEvent" );

						UnityEditor.EditorGUI.BeginChangeCheck();

						GUI.enabled = !requiresUse;
						UnityEditor.EditorGUILayout.PropertyField( prop1 );
						GUI.enabled = true;

						UnityEditor.EditorGUILayout.PropertyField( prop2, new GUIContent( requiresUse ? "On Use Event" : "On Stay Event" ) );

						GUI.enabled = !requiresUse;
						UnityEditor.EditorGUILayout.PropertyField( prop3 );
						GUI.enabled = true;

						if( UnityEditor.EditorGUI.EndChangeCheck() )
						{
							tv.ApplyModifiedProperties();
							foreach( TriggerVolume volume in triggerVolumes )
							{
								volume.onEnterEvent = onEnterEvent;
								volume.onStayEvent = onStayEvent;
								volume.onExitEvent = onExitEvent;
							}
							invalidate = true;
						}
					}
					GUILayout.EndVertical();

					UnityEditor.EditorGUI.indentLevel = 0;
				}
			}
			GUILayout.EndVertical();

			return invalidate;
		}

#endif

		/// <summary>
		/// Called when the volume is created in the editor.
		/// </summary>
		/// <param name="volume">The generated volume game object.</param>
		public override void OnCreateVolume( GameObject volume )
		{
			TriggerVolumeComponent tvc = volume.AddComponent<TriggerVolumeComponent>();
			tvc.triggerType = triggerType;
			tvc.useFilterTag = useFilterTag;
			tvc.filterTag = filterTag;
			tvc.layer = layer;
			tvc.triggerOnceOnly = triggerOnceOnly;
			tvc.requiresUse = requiresUse;
			tvc.inputEventName = inputEventName;
			tvc.inputKey = inputKey;
			tvc.useInputType = useInputType;
			tvc.onEnterEvent = onEnterEvent;
			tvc.onStayEvent = onStayEvent;
			tvc.onExitEvent = onExitEvent;
		}
	}
}

#endif
