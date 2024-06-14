entry:

count-commit:
	@count=$$(git shortlog -sn | cut -f1 | awk '{s+=$$1} END {print s}'); \
	echo "There are in total $$count commits in this repository.";