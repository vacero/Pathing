﻿using System.Runtime.CompilerServices;
using BhModule.Community.Pathing.Utility;
using TmfLib;
using AttributeCollection = TmfLib.Prototype.AttributeCollection;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardMarker {

        private const string ATTR_INVERTBEHAVIOR = "invert-behavior";

        public bool InvertBehavior { get; set; }

        /// <summary>
        /// invert-behavior
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Populate_InvertBehavior(AttributeCollection collection, IPackResourceManager resourceManager) {
            { if (collection.TryPopAttribute(ATTR_INVERTBEHAVIOR, out var attribute)) this.InvertBehavior = attribute.GetValueAsBool(); }
        }

    }
}