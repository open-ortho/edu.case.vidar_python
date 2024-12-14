"""
Simple function to only print a value if a function is defined as verbose.
"""
def verbose_print(value, verbosity):
    if verbosity:
        print(value)