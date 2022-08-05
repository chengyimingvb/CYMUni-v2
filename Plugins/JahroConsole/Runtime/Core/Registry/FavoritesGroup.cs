using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JahroConsole.Core.Registry
{
    internal class FavoritesGroup : SimpleGroup
    {

        internal FavoritesGroup() : base("Favorites")
        {
            Foldout = true;
            LIFO = true;
        }

        internal void CommandFavoriteChanged(ConsoleCommandEntry entry, bool favorite)
        {
            if (favorite)
            {
                AddCommandEntry(entry);
            }
            else
            {
                RemoveCommandEntry(entry);
            }
        }
    }
}