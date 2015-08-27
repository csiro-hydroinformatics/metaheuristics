#pragma once

#include <Rcpp.h>
#include <sce.hpp>

using namespace mhcpp::optimization;
using namespace mhcpp;

class Population
{
public:
	Population(const Rcpp::DataFrame& df, Rcpp::NumericVector minima, Rcpp::NumericVector maxima);	
	Population& operator=(const Population &src);
	Population& operator=(const Population&& src);
	Population(const Population &src);
	Population(const Population&& src);
	size_t size();
	HyperCube < double > at(size_t i);
private:
	std::vector<HyperCube < double >> points;
};

class Scores
{
public:
	Scores(const std::vector<IObjectiveScores<HyperCube < double >>>& points);
	Scores& operator=(const Scores &src);
	Scores& operator=(const Scores&& src);
	Scores(const Scores &src);
	Scores(const Scores&& src);
	size_t size();
	std::vector<IObjectiveScores<HyperCube < double >>> get_all();
	Rcpp::DataFrame get_as_dataframe();
private:
	std::vector<IObjectiveScores<HyperCube < double >>> points;
};


class ObjectiveWrapper;


class Objective
{
	friend class ObjectiveWrapper;
protected:
	Objective() : f("rt") // HACK
	{}
public:
	Objective(Rcpp::Function objFunc, Rcpp::CharacterVector objName, Rcpp::LogicalVector maximizable);
	Rcpp::NumericVector evaluate(Rcpp::NumericVector x);
	ObjectiveWrapper as_evaluator();
	Rcpp::NumericVector as_numvec(HyperCube < double > x);
	IObjectiveScores<HyperCube < double >> evaluate_point(HyperCube < double > x);
	Scores evaluate_all(Population pop);
private:
	Rcpp::Function f;
	std::string objName;
	bool maximizable;
};

class ObjectiveWrapper
	: public IObjectiveEvaluator < HyperCube<double> >
{
	friend class Objective;
	friend class Cplx;
protected:
	ObjectiveWrapper() {}
private:
	Objective obj;
public:
	ObjectiveWrapper(const Objective& obj) { this->obj = obj; }
	~ObjectiveWrapper() {}
	IObjectiveScores<HyperCube<double>> EvaluateScore(HyperCube<double> systemConfiguration);
};
