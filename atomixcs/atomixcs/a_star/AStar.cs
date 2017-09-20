﻿using System;
using System.Collections.Generic;

namespace atomixcs.a_star {
	class AStar {
		static float manhattan_distance(Node a, Node b) {
			return Math.Abs(a.position.x - b.position.x) + Math.Abs(a.position.y - b.position.y);
		}

		static float euclidean_distance(Node a, Node b) {
			return (float) Math.Sqrt(Math.Pow(a.position.x - b.position.x, 2) + Math.Pow((a.position.y - b.position.y), 2));
		}

		/**
		 * @Optimization: what would be a way of differentiating a closer to solved state from an unsolved/unoptimal state
		 * represented just by a real number?
		 **/
		static float state_heuristic(State current, State target) {
			float heuristic = 0;
			float mean_distance = 0;
			float distance = 0;

			for (int i = 0; i < current.items.Count && i < target.items.Count; i++) {
				heuristic += manhattan_distance(current.items[i], target.items[i]);
				distance = 0;

				for (int j = 0; j < current.items.Count; j++) {
					if (i != j) {
						distance += euclidean_distance(current.items[i], current.items[j]);
					}
				}

				mean_distance += distance / current.items.Count;
			}

			heuristic += mean_distance / current.items.Count;

			return heuristic;
		}

		/**
		 * @Optimization: replace this with proper priority queue so we can pop the lowest cost item more efficiently.
		 * 
		 * --Note: tried implementing this PriorityQueue: https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp
		 * but the results where x10 times slower than with this simple function, going from ~250 iterations to ~3700
		 * on the same example.
		 **/
		static State get_lowest_cost(List<State> list) {
			State node = list[0];

			for (int i = 1; i < list.Count; i++) {
				if (list[i].f_cost < node.f_cost) {
					node = list[i];
				}
			}

			return node;
		}

		static bool states_equal(State a, State b) {
			for (int i = 0; i < a.items.Count && i < b.items.Count; i++) {
				if (a.items[i].position != b.items[i].position) {
					return false;
				}
			}

			return true;
		}
		
		static bool contains_state(List<State> list, State current) {
			for (int i = 0; i < list.Count; i++) {
				if (states_equal(list[i], current)) {
					return true;
				}
			}

			return false;
		}

		public static List<State> a_star(Grid grid, State start_state, State target_state) {
			List<State> open_list = new List<State>();
			List<State> closed_list = new List<State>();

			List<State> path = new List<State>();

			State current_state;
			List<State> neighbouring_states;

			float cost;
			float heuristic;
			bool is_in_open;

			start_state.set_cost(0, state_heuristic(start_state, target_state));
			open_list.Add(start_state);

			int iteration_count = 0; // for testing only.

			while (open_list.Count > 0) {
				iteration_count++;
				
				current_state = get_lowest_cost(open_list);
				open_list.Remove(current_state);
				closed_list.Add(current_state);

				if (states_equal(current_state, target_state)) {
					Console.WriteLine("\n==============================================\n");
					Console.WriteLine("\nEND state:");

					grid.draw_grid(current_state);
					Console.WriteLine(current_state);
					Console.WriteLine();

					Console.WriteLine("Finished in: {0} iterations\n", iteration_count);
					return path;
				}

				// @Optimization: find a better way of detecting usable neighbour states.
				neighbouring_states = grid.expand_state(current_state);

				for (int i = 0; i < neighbouring_states.Count; i++) {
					if (!contains_state(closed_list, neighbouring_states[i])) {
						cost = current_state.cost + state_heuristic(current_state, neighbouring_states[i]);
						is_in_open = contains_state(open_list, neighbouring_states[i]);

						if (cost < neighbouring_states[i].cost || !is_in_open) {
							heuristic = state_heuristic(current_state, target_state);
							neighbouring_states[i].set_cost(cost, heuristic);

							/**
							 * @Important: we need a way of storing states and reconstructing a path.
							 * Currenlty the path list stores all states visited, not only the ones that are part of the solution.
							 * 
							 * In the original algorithm, Nodes would have a parent property and the parent would be set or overwritten
							 * with the new found optimal parent. However because we dont have a preset List of all States we cant do
							 * that. A possible solution would be a HashTable where the keys are the States and the values are their parent State.
							 * This way we can set/update the parents of a list of States and later reconstruct a path List.
							 **/
							path.Add(current_state);

							if (!is_in_open) {
								open_list.Add(neighbouring_states[i]);
							}
						}
					}
				}

				grid.draw_grid(current_state);
				Console.WriteLine(current_state);
				Console.WriteLine();
			}

			return path;
		}
	}
}