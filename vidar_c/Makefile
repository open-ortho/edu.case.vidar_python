# Define the compiler
CC = gcc

LIBUSB = /opt/homebrew/Cellar/libusb/1.0.27

# Define the flags for the compiler
CFLAGS = -Wall -I$(LIBUSB)/include/libusb-1.0

# Define linker flags (note the -L flag)
LDFLAGS = -L$(LIBUSB)/lib -lusb-1.0

# Define the target executable
TARGET = vidar_inquiry 

# Define the source files
SRCS =  vidar_inquiry.c

# Automatically generate the list of object files
OBJS = $(SRCS:.c=.o)

# The default target
all: $(TARGET)

# Link the target executable
$(TARGET): $(OBJS)
	$(CC) $(OBJS) -o $@ $(LDFLAGS)

# Compile the source files into object files
%.o: %.c
	$(CC) $(CFLAGS) -c $< -o $@

# Clean the build directory
clean:
	rm -f $(TARGET) $(OBJS)

.PHONY: all clean
