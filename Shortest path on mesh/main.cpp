#include <iostream>
#include <fstream>
#include <vector>
#include <list>
#include <queue>

using namespace std;

class state
{
public:
	int cost, level, last_node;
	vector<bool> used;
	state* father;
	vector<vector<int>> matrix;

	state(vector<bool>& used, int cost = 0, state* father = NULL)
	{
		this->cost = cost;
		this->father = father;
		this->used = used;
		last_node = 0;
		level = 1;
	}

};

struct customCmp
{
	bool operator()(const state* a, const state* b)
	{
		return a->cost > b->cost;
	}
};

void print_sol(state* st)
{
	if (st == NULL)
		return;

	print_sol(st->father);
	std::cout << st->last_node + 1 << " ";
}

int reduction(vector<vector<int>>& matrix)
{
	int n = matrix.size(), red = 0;
	for (int i = 0; i < n; i++)
	{
		int min = -1;
		for (int j = 0; j < n; j++)
		{
			if (matrix[i][j] != -1 && (matrix[i][j] < min || min == -1))
			{
				min = matrix[i][j];
			}
		}

		//nothing to do
		if (min <= 0)
			continue;

		red += min;
		for (int j = 0; j < n; j++)
		{
			if (matrix[i][j] != -1)
				matrix[i][j] -= min;
		}
	}

	for (int i = 0; i < n; i++)
	{
		int min = -1;
		for (int j = 0; j < n; j++)
		{
			if (matrix[j][i] != -1 && (matrix[j][i] < min || min == -1))
			{
				min = matrix[j][i];
			}
		}

		//nothing to do
		if (min <= 0)
			continue;

		red += min;
		for (int j = 0; j < n; j++)
		{
			if (matrix[j][i] != -1)
				matrix[j][i] -= min;
		}
	}

	return red;
}

void print_matrix(vector<vector<int>> matrix)
{
	for (int i = 0; i < matrix.size(); i++)
	{
		for (int j = 0; j < matrix[i].size(); j++)
		{
			cout << matrix[i][j] << " ";
		}
		cout << endl;
	}
	cout << flush;
}

void problema4(const GLfloat vf_pos[])
{
	priority_queue<state*, vector<state*>, customCmp> min_heap;
	vector<vector<int>> matrix;
	ifstream fin("date.in");
	int n = nr_puncte, red;
	matrix.resize(n);

	//read adj matrix
	for (int i = 0; i < n; i++)
	{
		int point, nr_neigh;
		fin >> point;
		matrix[point].resize(n, -1);

		fin >> nr_neigh;

		for (int j = 0; j < nr_neigh; j++)
		{
			int x;
			fin >> x;
			matrix[i][x] = x;
		}
	}

	print_matrix(matrix);

	//reduction
	red = reduction(matrix);

	vector<bool> used(n, 0);
	used[0] = 1;
	//add initial state to min_heap
	state* initial_state = new state(used);
	initial_state->matrix = matrix;
	initial_state->cost = red;

	min_heap.push(initial_state);

	//to be used after finding solutions
	int best_sol = -1;
	state* best_state = NULL;

	//Extract states one by one
	while (!min_heap.empty())
	{
		//take best state out
		state* selected_state = min_heap.top();
		min_heap.pop();

		//poped bad node
		if (selected_state->cost > best_sol && best_sol != -1)
		{
			delete(selected_state);
			continue;
		}

		//compute all of its children that can yield solutions and add them to min_heap
		for (int i = 1; i < n; i++)
		{
			//jump over used nodes
			if (selected_state->used[i])
				continue;

			state* child = new state(selected_state->used, selected_state->cost, selected_state);
			child->level = selected_state->level + 1;
			child->matrix = selected_state->matrix;
			child->last_node = i;

			//last level node, we can compute real cost and add as solution
			if (selected_state->level == n - 1)
			{
				child->cost += child->matrix[selected_state->last_node][i] + child->matrix[i][0];

				if (child->cost < best_sol || best_sol == -1)
				{
					best_sol = child->cost;
					best_state = child;
				}
			}
			else
			{
				//line i is -1
				for (int j = 0; j < n; j++)
					child->matrix[selected_state->last_node][j] = -1;

				//column j is -1
				for (int j = 0; j < n; j++)
					child->matrix[j][i] = -1;

				//dont go back
				child->matrix[i][0] = -1;

				//reduce matrix
				child->cost += reduction(child->matrix);

				//not last level node, we compute cost an insert it in min_heap
				child->cost += selected_state->matrix[selected_state->last_node][i];

				//check if child can make a better solution
				if (child->cost >= best_sol && best_sol != -1)
				{
					delete(child);
					continue;
				}

				child->used[i] = true;
				min_heap.push(child);
			}
		}

	}

	//print solution
	cout << "Cost minim:" << best_sol << endl;
	print_sol(best_state);
	cout << "1" << endl;

}