// -*- mode: C++; c-indent-level: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
//
// rcpp_module.cpp: Rcpp R/C++ interface class library -- Rcpp Module examples
//
// Copyright (C) 2010 - 2012  Dirk Eddelbuettel and Romain Francois
//
// This file is part of Rcpp.
//
// Rcpp is free software: you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
//
// Rcpp is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Rcpp.  If not, see <http://www.gnu.org/licenses/>.

#include "rcpp_module.h"

std::string hello() {
    throw std::range_error( "boom" ) ;
}

using namespace mhcpp::optimization;
using namespace mhcpp;

// std::vector<IObjectiveScores<T>>
Population::Population(const Rcpp::DataFrame& df, Rcpp::NumericVector minima, Rcpp::NumericVector maxima)
{
	auto nr = df.nrows();
	int nc = df.size();
	auto names = Rcpp::as<std::vector<std::string>>(df.names());
	for (size_t row = 0; row < nr; row++)
	{
		HyperCube < double > p;
		for (size_t col = 0; col < nc; col++)
		{
			Rcpp::NumericVector v = df[names[col]];
			p.Define(names[col], minima[col], maxima[col], v[row]);
		}
		points.push_back(p);
	}
}

Population& Population::operator=(const Population &src)
{
	if (&src == this){
		return *this;
	}
	this->points = src.points;
	return *this;
}

Population& Population::operator=(const Population&& src)
{
	if (&src == this){
		return *this;
	}
	points = std::move(src.points);
	return *this;
}

Population::Population(const Population &src)
{
	this->points = src.points;
}

Population::Population(const Population&& src)
{
	points = std::move(src.points);
}

size_t Population::size()
{
	return points.size();
}

HyperCube < double > Population::at(size_t i)
{
	return points[i];
}

Scores::Scores(const std::vector<IObjectiveScores<HyperCube < double >>>& points)
{
	this->points = points;
}

Scores& Scores::operator=(const Scores &src)
{
	if (&src == this){
		return *this;
	}
	this->points = src.points;
	return *this;
}

Scores& Scores::operator=(const Scores&& src)
{
	if (&src == this){
		return *this;
	}
	points = std::move(src.points);
	return *this;
}

Scores::Scores(const Scores &src)
{
	this->points = src.points;
}

Scores::Scores(const Scores&& src)
{
	points = std::move(src.points);
}

size_t Scores::size()
{
	return points.size();
}

std::vector<IObjectiveScores<HyperCube < double >>> Scores::get_all()
{
	return points;
}

Rcpp::DataFrame Scores::get_as_dataframe()
{
	int nrows = points.size();
	if(nrows == 0)
		return Rcpp::DataFrame(0);
	int pDim = points[0].SystemConfiguration().Dimensions();
	auto pNames = points[0].SystemConfiguration().GetVariableNames();
	int sDim = points[0].ObjectiveCount();
	
	std::vector<Rcpp::NumericVector> parameters;
	std::vector<Rcpp::NumericVector> objectives;

	std::vector<std::string> colNames;
	for (int i = 0; i < pDim; ++i)
	{
		colNames.push_back(pNames[i]); 
		parameters.push_back(Rcpp::NumericVector(nrows));
		// Rprintf( "parameters[%d].size() = %d\n", i, parameters[i].size());
	}
	for (int i = 0; i < sDim; ++i)
	{
		colNames.push_back("obj");
		objectives.push_back(Rcpp::NumericVector(nrows));
		// Rprintf( "objectives[%d].size() = %d\n", i, objectives[i].size());
	}
	for (int r = 0; r < nrows; ++r)
	{
		auto p = points[r];
		auto cfg = p.SystemConfiguration();
		for (int i = 0; i < pDim; ++i)
			parameters[i][r] = cfg.GetValue(pNames[i]);
		for (int i = 0; i < sDim; ++i)
			objectives[i][r] = p.Value(i);
	}
	// Rprintf( "Done setting vectors of values\n");
	
	//http://stackoverflow.com/questions/8631197/constructing-a-data-frame-in-rcpp
	Rcpp::List returned_frame(pDim + sDim);

	for (int i = 0; i < pDim; ++i)
		returned_frame[i] = parameters[i];
	// Rprintf( "Done assigning data frames parameter vectors.\n");
	for (int i = 0; i < sDim; ++i)
		returned_frame[i+pDim] = objectives[i];
	// Rprintf( "Done assigning data frames objectives vectors.\n");
	
	Rcpp::StringVector col_names = Rcpp::wrap(colNames);
	returned_frame.attr("names") = col_names;

	return Rcpp::DataFrame(returned_frame);
}


IObjectiveScores<HyperCube<double>> ObjectiveWrapper::EvaluateScore(HyperCube<double> systemConfiguration)
{
	return obj.evaluate_point(systemConfiguration);
}

Objective::Objective(Rcpp::Function objFunc, Rcpp::CharacterVector objName, Rcpp::LogicalVector maximizable) : f(objFunc)
{
	this->objName = objName[0];
	this->maximizable = maximizable[0];
}

Rcpp::NumericVector Objective::evaluate(Rcpp::NumericVector x)
{
	return f(x);
}

ObjectiveWrapper Objective::as_evaluator()
{
	return ObjectiveWrapper(*this);
}

Rcpp::NumericVector Objective::as_numvec(HyperCube < double > x)
{
	auto pNames = x.GetVariableNames();
	Rcpp::NumericVector nv(pNames.size());
	for (int i = 0; i < pNames.size(); ++i)
		nv[i] = x.GetValue(pNames[i]);

	Rcpp::StringVector col_names = Rcpp::wrap(pNames);
	nv.attr("names") = col_names;
	return nv;
}

IObjectiveScores<HyperCube < double >> Objective::evaluate_point(HyperCube < double > x)
{
	Rcpp::NumericVector vec = as_numvec(x);
	Rcpp::NumericVector obj = evaluate(vec);
	return IObjectiveScores<HyperCube < double >>(x, objName, obj[0], maximizable);
}

Scores Objective::evaluate_all(Population pop)
{
	std::vector<IObjectiveScores<HyperCube < double >>> s;
	for (size_t i = 0; i < pop.size(); i++)
	{
		auto point = pop.at(i);
		s.push_back(evaluate_point(point));
	}
	return Scores(s);
}	

class Cplx : public Complex<HyperCube < double >> {
public:
	Cplx(Population pop, Objective f, int q, int seed) : 
		Complex(f.evaluate_all(pop).get_all(), nullptr, q, seed)
	{
		this->w = f.as_evaluator();
		evaluator = &w;
	}

	~Cplx() {}

	void evolve() 
	{
		Complex::Evolve();
	}
	Scores get_population() 
	{
		return Scores(GetObjectiveScores());
	}

private:
	ObjectiveWrapper w;
};

RCPP_EXPOSED_CLASS(Population)
RCPP_EXPOSED_CLASS(Objective)
RCPP_EXPOSED_CLASS(Scores)

RCPP_MODULE(mh){
    using namespace Rcpp ;

	class_<Objective>("Objective")
		.constructor<Rcpp::Function, Rcpp::CharacterVector, Rcpp::LogicalVector>()
		.method("evaluate", &Objective::evaluate, "Evaluates the score given this objective and a point")
		;
	// with formal arguments specification
	//Rcpp::function("foo", &foo,
	//	List::create(_["x"] = 1, _["y"] = 1.0),
	//	"documentation for foo ");

	class_<Population>("Population")
		.constructor<Rcpp::DataFrame, Rcpp::NumericVector, Rcpp::NumericVector>();

	class_<Scores>("Scores")
	.method("as_dataframe", &Scores::get_as_dataframe, "Launch the evolution of the complex")
	;

    class_<Cplx>("Complex")
    .constructor<Population, Objective, int, int>()
	.method("evolve", &Cplx::evolve, "Launch the evolution of the complex")
	.method("get_population", &Cplx::get_population, "Gets the population of the complex")
    ;


	Rcpp::function("hello", &hello, "documentation for hello ");

}


