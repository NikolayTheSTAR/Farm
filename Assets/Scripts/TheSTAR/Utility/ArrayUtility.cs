using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TheSTAR.Utility
{
    public static class ArrayUtility
    {
        // Fast find an element with type TNeeded among elements with base type T
        public static int FastFindElement<T, TNeeded>(T[] array)
            where T: IComparableType<T> // T - base type for all elements
            where TNeeded : T // TNeeded - current type for needed element
        {
            int 
            minBorder = -1, // exclusive
            maxBorder = array.Length, // exclusive
            index = -1,
            maxIterationCount = 100,
            iterationIndex = 0;

            bool 
            toBigger = true; // from 0 to n

            T element;
            
            while (iterationIndex < maxIterationCount)
            {
                iterationIndex++;
                
                index = (maxBorder + minBorder) / 2;
                element = array[index];

                if (element is TNeeded) return index;
                else
                {
                    toBigger = element.CompareToType<TNeeded>() < 0;

                    if (toBigger) minBorder = index;
                    else maxBorder = index;
                }
            }

            return -1;
        }

        public static int FastFindElement<T>(T[] array, T neededElement) where T : IComparable<T>
        {
            int 
            minBorder = -1, // exclusive
            maxBorder = array.Length, // exclusive
            index = -1,
            maxIterationCount = 100,
            iterationIndex = 0;

            bool 
            toBigger = true; // from 0 to n

            T element;
            
            while (iterationIndex < maxIterationCount)
            {
                iterationIndex++;
                
                index = (maxBorder + minBorder) / 2;
                element = array[index];

                if (element.Equals(neededElement)) return index;
                else
                {
                    toBigger = element.CompareTo(neededElement) < 0;

                    if (toBigger) minBorder = index;
                    else maxBorder = index;
                }
            }

            return -1;
        }

        public static string GetStringFromEnumerable(IEnumerable enumerable)
        {
            StringBuilder sb = new();
            sb.AppendLine("Values:");
            sb.AppendLine();
            
            foreach (var element in enumerable)
            {
                if (element == null) sb.AppendLine("null");
                else sb.AppendLine(element.ToString());
            }
            
            return sb.ToString();
        }

        public static void PrintEnumerable(IEnumerable enumerable)
        {
            Debug.Log(GetStringFromEnumerable(enumerable));
        }

        public static bool IsNullOfEmpty<T>(T[] array) => array == null || array.Length == 0;
    
        public static T[] UpdateArraySize<T>(T[] array, int size)
        {
            var tempClone = new T[size]; // create a new array with new size

            for (int x = 0; x < size; x++)
            {
                if (x >= array.GetLength(0)) return tempClone;

                tempClone[x] = array[x];
            }

            return tempClone;
        }

        public static T[,] UpdateArraySize<T>(T[,] array, int width, int height)
        {
            array ??= new T[0, 0];

            var tempClone = new T[width, height]; // create a new array with new size

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    if (x >= array.GetLength(0) && y >= array.GetLength(1)) return tempClone;
                    if (x >= array.GetLength(0) || y >= array.GetLength(1)) break;

                    tempClone[x, y] = array[x, y];
                }
            }
            
            return tempClone;
        }

        public static T[,,] UpdateArraySize<T>(T[,,] array, int width, int height, int depth)
        {
            array ??= new T[0, 0, 0];

            var tempClone = new T[width, height, depth]; // create a new array with new size

            Parallel.For(0, array.GetLength(2), z =>
            {
                for (var y = 0; y < array.GetLength(1); y++)
                {
                    for (var x = 0; x < array.GetLength(0); x++)
                    {
                        tempClone[x, y, z] = array[x, y, z];
                    }
                }
            });
            
            return tempClone;
        }
    }

    public interface IComparableType<in T>
    {
        int CompareToType<T1>() where T1 : T;
    }
}