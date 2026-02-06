using UnityEngine;

namespace Runtime.Application
{
    public interface Door
    {
        void CrossTo(Transform objectToMove);
    }
}