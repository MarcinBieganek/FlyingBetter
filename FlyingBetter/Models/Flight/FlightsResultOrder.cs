using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FlyingBetter.Models.Flight
{
    public class FlightsResultOrder
    {
        public int FlightValue(FlightsResult flight)
        {
            int value = -flight.price;
            // flights without layovers are better
            if (flight.transfers == 0)
            {
                value += 100;
            }
            else
            {
                value -= (flight.transfers * 50);
            }
            // flights during a day are better
            TimeSpan dayStart = new TimeSpan(8, 0, 0);
            TimeSpan dayEnd = new TimeSpan(20, 0, 0);
            if (dayStart <= flight.departure_at.TimeOfDay && flight.departure_at.TimeOfDay <= dayEnd)
            {
                value += 100;
            }
            // last minute flights are worst
            TimeSpan oneMonth = new TimeSpan(30, 0, 0, 0);
            TimeSpan fourMonths = new TimeSpan(120, 0, 0, 0);
            TimeSpan nowToFlightDiff = flight.departure_at - DateTime.Now;
            if (nowToFlightDiff < oneMonth)
            {
                value += 30;
            }
            else if (oneMonth <= nowToFlightDiff && nowToFlightDiff <= fourMonths)
            {
                value += 100;
            }
            else
            {
                value += 50;
            }

            return value;
        }

        List<FlightsResult> orderFlights(List<FlightsResult> flights)
        {
            return flights
                    .OrderByDescending(f => FlightValue(f))
                    .ToList();
        }

        List<FlightsResult> limitFlights(List<FlightsResult> flights, int limit)
        {
            return flights
                    .Take(limit)
                    .ToList();
        }

        public void orderFlightResults(FlightSearchResultModel model)
        {
            model.flightsResults = orderFlights(model.flightsResults);
            model.flightsBackResults = orderFlights(model.flightsBackResults);
        }

        public void orderFlightResults(FlightIdeasModel model)
        {
            model.FlightsResults = orderFlights(model.FlightsResults);
        }

        public void limitFlightResults(FlightIdeasModel model, int limit)
        {
            model.FlightsResults = limitFlights(model.FlightsResults, limit);
        }
    }
}