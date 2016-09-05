using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LunchBot.Model;

namespace LunchBot.Data
{


    [Serializable]
    public class DataStorage
    {
        private static List<Restaurant> _restaurantList;
        private static List<Restaurant> _restaurantListLine;
        private static List<Restaurant> _restaurantListWait;
        private static List<User> _userList;

        public static List<Restaurant> RestaurantList
        {
            get { return _restaurantList ?? (_restaurantList = new List<Restaurant>()); }
            set
            {
                _restaurantList = value;
            }
        }
        public static List<Restaurant> InLineRestaurantList
        {
            get { return _restaurantListLine ?? (_restaurantListLine = new List<Restaurant>()); }
            set
            {
                _restaurantListLine = value;
            }
        }
        public static List<Restaurant> WaitingList
        {
            get { return _restaurantListWait ?? (_restaurantListWait = new List<Restaurant>()); }
            set
            {
                _restaurantListWait = value;
            }
        }

        public static List<User> UserList
        {
            get { return _userList ?? (_userList = new List<User>()); }
            set
            {
                _userList = value;
            }
        }
    }
}