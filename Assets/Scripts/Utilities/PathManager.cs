using System.IO;
using System.Linq;
using UnityEngine;

namespace Utilities {
    public static class PathManager {
        private static char _separator = Path.AltDirectorySeparatorChar;
        private static string _prefabDir = "Prefabs";
        private static string _robotComponentDir = "RobotComponent";
        private static string RobotComponentPath => $"{_prefabDir}{_separator}{_robotComponentDir}";
        public static string ChassisPath => $"{RobotComponentPath}{_separator}Chassis";
        public static string WeaponPath => $"{RobotComponentPath}{_separator}Weapon";
        public static string WheelPath => $"{RobotComponentPath}{_separator}Wheel";
        public static string GadgetPath => $"{RobotComponentPath}{_separator}Gadget";
        
        public  static string GetComponentAssetPath(GameObject robotComponent) {
            var componentName = robotComponent.name;
            var directories = componentName.Split(".").SkipLast(1).ToArray();
            var path = directories.Aggregate(RobotComponentPath, 
                (current, dir) => current + $"{_separator}{dir}");
            path += $"{_separator}{componentName}";
            //Debug.Log(path);
            return path;
        }
    }
}